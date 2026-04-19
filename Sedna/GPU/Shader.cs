// SPDX-FileCopyrightText: 2026 Neptuwunium <ada@chronovore.dev>
//
// SPDX-License-Identifier: EUPL-1.2

using System.Buffers;
using System.Text;
using SDL;
using static SDL.SDL3;
using static SDL.SDL3ShaderCross;

namespace Sedna.GPU;

public record struct ShaderOptions {
	public static ShaderOptions Default { get; } = new();

	public bool Debug { get; set; }
	public string? DebugName { get; set; }
	public bool CullUnusedBindings { get; set; }
}

public class GraphicShader : Shader {
	internal GraphicShader(ShaderResourceId value, ResourceManager manager) : base(value, manager) { }

	public int Samplers { get; set; }
	public int Textures { get; set; }
	public int StorageBuffers { get; set; }
	public int UniformBuffers { get; set; }

	protected override unsafe void CreateShader(byte* pointer, nuint size, byte* entryPoint) {
		var shaderInfo = stackalloc SDL_GPUShaderCreateInfo[1];
		shaderInfo->code = pointer;
		shaderInfo->code_size = size;
		shaderInfo->entrypoint = entryPoint;
		shaderInfo->stage = Stage switch {
			SDL_ShaderCross_ShaderStage.SDL_SHADERCROSS_SHADERSTAGE_VERTEX => SDL_GPUShaderStage.SDL_GPU_SHADERSTAGE_VERTEX,
			SDL_ShaderCross_ShaderStage.SDL_SHADERCROSS_SHADERSTAGE_FRAGMENT => SDL_GPUShaderStage.SDL_GPU_SHADERSTAGE_FRAGMENT,
			SDL_ShaderCross_ShaderStage.SDL_SHADERCROSS_SHADERSTAGE_COMPUTE => throw new InvalidOperationException(),
			_ => throw new InvalidOperationException(),
		};
		shaderInfo->num_samplers = (uint) Samplers;
		shaderInfo->num_storage_textures = (uint) Textures;
		shaderInfo->num_storage_buffers = (uint) StorageBuffers;
		shaderInfo->num_uniform_buffers = (uint) UniformBuffers;
		shaderInfo->props = 0;

		// todo SDL_PROP_GPU_SHADER_CREATE_NAME_STRING

		DeviceShader = SDL_CreateGPUShader(Manager.Scene.Renderer.DeviceHandle, shaderInfo);
	}
}

public class ComputeShader : Shader {
	public ComputeShader(ShaderResourceId value, ResourceManager manager) : base(value, manager) { }

	protected override unsafe void CreateShader(byte* pointer, nuint size, byte* entryPoint) => throw new NotImplementedException();
}

public abstract class Shader : ManagedResource<ShaderResourceId> {
	static Shader() {
		if (SDL_ShaderCross_Init() == 0) {
			return;
		}

		AppDomain.CurrentDomain.ProcessExit += (_, _) => {
			SDL_ShaderCross_Quit();
		};
	}

	internal Shader(ShaderResourceId value, ResourceManager manager) : base(value, manager) { }

	public string? Name { get; set; }
	public Dictionary<string, string?> Defines { get; set; } = [];
	public SDL_ShaderCross_ShaderStage Stage { get; set; }
	public string? IncludeDir { get; set; }
	public string EntryPoint { get; set; } = "shader_main";
	public string? ShaderCode { get; set; }
	public ShaderOptions Options { get; set; } = ShaderOptions.Default;

	public nint DeviceShaderCode { get; set; }
	public nuint DeviceShaderSize { get; set; }
	public unsafe SDL_GPUShader* DeviceShader { get; set; }

	protected abstract unsafe void CreateShader(byte* pointer, nuint size, byte* entryPoint);

	public override unsafe void Create() {
		if (DeviceShader != null) {
			return;
		}
		
		if (ShaderCode == null) {
			// todo: logging
			return;
		}

		const int MAX_SIZE = 4096;

		// todo: ShaderCache
		var info = stackalloc SDL_ShaderCross_HLSL_Info[1];
		var size = stackalloc nuint[1];

		byte[]? entryPointArray = null;
		byte[]? textArray = null;
		byte[]? includeDirArray = null;
		byte[]? defineTextArray = null;

		var codePtr = DeviceShaderCode;
		var codeSize = DeviceShaderSize;

		try {
			// @formatter:off
			var entryPointSize = Encoding.UTF8.GetByteCount(EntryPoint) + 1;
			var entryPointBuffer = entryPointSize > MAX_SIZE ?
				entryPointArray = ArrayPool<byte>.Shared.Rent(entryPointSize) :
				stackalloc byte[entryPointSize];
			entryPointBuffer.Clear();
			Encoding.UTF8.GetBytes(EntryPoint, entryPointBuffer);
			// @formatter:on

			if (codePtr == nint.Zero || codeSize == nuint.Zero) {
				if (codePtr != nint.Zero) {
					SDL_free(codePtr);
					DeviceShaderCode = nint.Zero;
				}

				// @formatter:off
				var textSize = Encoding.UTF8.GetByteCount(ShaderCode) + 1;
				var textBuffer = textSize > MAX_SIZE ?
					textArray = ArrayPool<byte>.Shared.Rent(textSize) :
					stackalloc byte[textSize];

				var includeDirSize = IncludeDir != null ? 
					Encoding.UTF8.GetByteCount(IncludeDir) + 1 :
					0;
				var includeDirBuffer = includeDirSize != 0 ? 
					includeDirSize > MAX_SIZE ? 
						includeDirArray = ArrayPool<byte>.Shared.Rent(includeDirSize) : 
						stackalloc byte[includeDirSize] : 
					Span<byte>.Empty;

				var defineTextSize = Defines.Count > 0 ? 
					Defines.Sum(x => Encoding.ASCII.GetByteCount(x.Key) + 1 + (x.Value != null ? Encoding.ASCII.GetByteCount(x.Value) + 1 : 0)) :
					0;
				var defineTextBuffer = defineTextSize != 0 ? 
					defineTextSize > MAX_SIZE ? 
						defineTextArray = ArrayPool<byte>.Shared.Rent(defineTextSize) : 
						stackalloc byte[defineTextSize] :
					Span<byte>.Empty;
				var defineBuffer = Defines.Count != 0 ? 
					stackalloc SDL_ShaderCross_HLSL_Define[Defines.Count + 1] : 
					Span<SDL_ShaderCross_HLSL_Define>.Empty;
				// @formatter:on

				textBuffer.Clear();
				includeDirBuffer.Clear();
				defineTextBuffer.Clear();
				defineBuffer.Clear();

				Encoding.UTF8.GetBytes(ShaderCode, textBuffer);

				if (includeDirSize > 0) {
					Encoding.UTF8.GetBytes(IncludeDir, includeDirBuffer);
				}

				if (defineTextSize > 0) {
					var definePos = 0;
					var defineIndex = 0;
					foreach (var (defineKey, defineValue) in Defines) {
						defineBuffer[defineIndex].name = (byte*) definePos;
						definePos += Encoding.ASCII.GetBytes(defineKey, defineTextBuffer[definePos..]) + 1;

						if (defineValue != null) {
							defineBuffer[defineIndex].value = (byte*) definePos;
							definePos += Encoding.ASCII.GetBytes(defineValue, defineTextBuffer[definePos..]) + 1;
						}

						defineIndex++;
					}

					defineBuffer[^1].name = null;
					defineBuffer[^1].value = null;
				}

				fixed (byte* code = textBuffer)
				fixed (byte* entryPoint = entryPointBuffer)
				fixed (byte* includeDir = includeDirBuffer)
				fixed (byte* defineText = defineTextBuffer) {
					foreach (ref var define in defineBuffer[..^1]) {
						define.name = defineText + (int) define.name;

						if (define.value != null) {
							define.value = defineText + (int) define.value;
						}
					}

					fixed (SDL_ShaderCross_HLSL_Define* defines = &defineBuffer.GetPinnableReference()) {
						info->source = code;
						info->entrypoint = entryPoint;
						info->include_dir = includeDirSize > 0 ? includeDir : null;
						info->defines = defines;
						info->shader_stage = Stage;
						var props = SDL_CreateProperties();
						SDL_SetBooleanProperty(props, SDL_SHADERCROSS_PROP_SHADER_DEBUG_ENABLE_BOOLEAN, true);
						try {
							info->props = (uint) props;
							// todo: ShaderOptions

							var ptr = SDL_ShaderCross_CompileSPIRVFromHLSL(info, size);
							if (ptr == nint.Zero) {
								// todo: logging
								return;
							}

							if (size[0] > int.MaxValue) {
								// todo: logging
								SDL_free(ptr);
								return;
							}

							codePtr = ptr;
							codeSize = size[0];
							if (OperatingSystem.IsMacOS()) {
								var mslInfo = stackalloc SDL_ShaderCross_SPIRV_Info[1];
								mslInfo->bytecode = (byte*) ptr;
								mslInfo->bytecode_size = size[0];
								mslInfo->entrypoint = entryPoint;
								mslInfo->shader_stage = Stage;
								mslInfo->props = 0;
								codePtr = SDL_ShaderCross_TranspileMSLFromSPIRV(mslInfo);
								SDL_free(ptr);
								if (codePtr == nint.Zero) {
									return;
								}

								codeSize = SDL_strlen((byte*) codePtr);
							}
						} finally {
							SDL_DestroyProperties(props);
						}
					}
				}
			}

			fixed (byte* entryPoint = entryPointBuffer) {
				CreateShader((byte*) codePtr, codeSize, entryPoint);
			}

			if (DeviceShader != null) {
				DeviceShaderCode = codePtr;
				DeviceShaderSize = codeSize;
			}
		} catch {
			if (codePtr != DeviceShaderCode && codePtr != nint.Zero) {
				SDL_free(codePtr);
			}

			if (DeviceShader != null) {
				Destroy();
			}
		} finally {
			if (entryPointArray != null) {
				ArrayPool<byte>.Shared.Return(entryPointArray);
			}
			
			if (textArray != null) {
				ArrayPool<byte>.Shared.Return(textArray);
			}
			
			if (includeDirArray != null) {
				ArrayPool<byte>.Shared.Return(includeDirArray);
			}
			
			if (defineTextArray != null) {
				ArrayPool<byte>.Shared.Return(defineTextArray);
			}
		}
	}

	public override unsafe void Destroy() {
		if (DeviceShader != null) {
			SDL_ReleaseGPUShader(Manager.Scene.Renderer.DeviceHandle, DeviceShader);
			DeviceShader = null;
		}

		if (DeviceShaderCode != nint.Zero) {
			SDL_free(DeviceShaderCode);
			DeviceShaderCode = nint.Zero;
			DeviceShaderSize = nuint.Zero;
		}
	}
}
