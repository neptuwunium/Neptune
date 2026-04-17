// SPDX-FileCopyrightText: 2026 Neptuwunium <ada@chronovore.dev>
//
// SPDX-License-Identifier: EUPL-1.2

using Pluto.SourceGen.TransparentStructGenerator;

namespace Sedna;

public interface IResourceId {
	ulong Value { get; }
	ResourceKind Kind { get; }
}

public enum ResourceKind : byte {
	Invalid = 0,
	Texture = 0x54, // 'T'
	Shader = 0x53, // 'S'
	Mesh = 0x4d, // 'M'
	Material = 0x49, // 'I'
}

[TransparentStruct<ulong>] public partial struct TextureResourceId : IResourceId {
	public ResourceKind Kind => (ResourceKind)(Value >> 56);
}

[TransparentStruct<ulong>] public partial struct ShaderResourceId : IResourceId {
	public ResourceKind Kind => (ResourceKind)(Value >> 56);
}

[TransparentStruct<ulong>] public partial struct MaterialResourceId : IResourceId {
	public ResourceKind Kind => (ResourceKind)(Value >> 56);
}

[TransparentStruct<ulong>] public partial struct MeshResourceId : IResourceId {
	public ResourceKind Kind => (ResourceKind)(Value >> 56);
}
