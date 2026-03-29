// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using Triton.Pixel;
using Triton.Pixel.Formats;

namespace Triton;

public static class PixelOperations {
	public static float Epsilon { get; set; } = 1e-05f;
}

[SuppressMessage("ReSharper", "InconsistentNaming")]
public static class PixelOperations<TColor, T>
	where TColor : unmanaged, IColor<TColor, T>, IColor
	where T : unmanaged, INumberBase<T>, IMinMaxValue<T> {
	public static void BlendPixel(PixelOperation operation, TColor srcPixel, ref TColor dstPixel) {
		switch (operation) {
			case PixelOperation.Copy: {
				CopyPixel(srcPixel, ref dstPixel);
				break;
			}
			case PixelOperation.AlphaBlend: {
				AlphaBlendPixel(srcPixel, ref dstPixel);
				break;
			}
			case PixelOperation.SrcOver: {
				OverPixel(srcPixel, ref dstPixel);
				break;
			}
			case PixelOperation.DstOver: {
				OverPixel(dstPixel, ref srcPixel);
				CopyPixel(srcPixel, ref dstPixel);
				break;
			}
			case PixelOperation.SrcIn: {
				InPixel(srcPixel, ref dstPixel);
				break;
			}
			case PixelOperation.DstIn: {
				InPixel(dstPixel, ref srcPixel);
				CopyPixel(srcPixel, ref dstPixel);
				break;
			}
			case PixelOperation.SrcOut: {
				OutPixel(srcPixel, ref dstPixel);
				break;
			}
			case PixelOperation.DstOut: {
				OutPixel(dstPixel, ref srcPixel);
				CopyPixel(srcPixel, ref dstPixel);
				break;
			}
			case PixelOperation.SrcAtop: {
				AtopPixel(srcPixel, ref dstPixel);
				break;
			}
			case PixelOperation.DstAtop: {
				AtopPixel(dstPixel, ref srcPixel);
				CopyPixel(srcPixel, ref dstPixel);
				break;
			}
			case PixelOperation.Xor: {
				XorPixel(srcPixel, ref dstPixel);
				break;
			}
			case PixelOperation.Plus: {
				PlusPixel(srcPixel, ref dstPixel);
				break;
			}
			case PixelOperation.Multiply: {
				MultiplyPixel(srcPixel, ref dstPixel);
				break;
			}
			case PixelOperation.Screen: {
				ScreenPixel(srcPixel, ref dstPixel);
				break;
			}
			case PixelOperation.Overlay: {
				OverlayPixel(srcPixel, ref dstPixel);
				break;
			}
			case PixelOperation.Darken: {
				DarkenPixel(srcPixel, ref dstPixel);
				break;
			}
			case PixelOperation.Lighten: {
				LightenPixel(srcPixel, ref dstPixel);
				break;
			}
			case PixelOperation.ColorDodge: {
				ColorDodgePixel(srcPixel, ref dstPixel);
				break;
			}
			case PixelOperation.ColorBurn: {
				ColorBurnPixel(srcPixel, ref dstPixel);
				break;
			}
			case PixelOperation.HardLight: {
				HardLightPixel(srcPixel, ref dstPixel);
				break;
			}
			case PixelOperation.SoftLight: {
				SoftLightPixel(srcPixel, ref dstPixel);
				break;
			}
			case PixelOperation.Difference: {
				DifferencePixel(srcPixel, ref dstPixel);
				break;
			}
			case PixelOperation.Exclusion: {
				ExclusionPixel(srcPixel, ref dstPixel);
				break;
			}
			default: throw new ArgumentOutOfRangeException(nameof(operation), operation, null);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void PremultiplyPixel(ref TColor srcPixel) {
		var tmpPixel = srcPixel.Convert<TColor, T, ColorRGBA<float>, float>();
		var vec = new Vector3(tmpPixel.R, tmpPixel.G, tmpPixel.B);
		vec *= tmpPixel.A;
		srcPixel = new ColorRGBA<float>(vec.X, vec.Y, vec.Z, tmpPixel.A).Convert<ColorRGBA<float>, float, TColor, T>();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void UnmultiplyPixel(ref TColor srcPixel) {
		var tmpPixel = srcPixel.Convert<TColor, T, ColorRGBA<float>, float>();
		var vec = new Vector3(tmpPixel.R, tmpPixel.G, tmpPixel.B);
		vec /= tmpPixel.A;
		srcPixel = new ColorRGBA<float>(vec.X, vec.Y, vec.Z, tmpPixel.A).Convert<ColorRGBA<float>, float, TColor, T>();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void CopyPixel(TColor srcPixel, ref TColor dstPixel) => dstPixel = srcPixel;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static (Vector3 Sc, float Sa, Vector3 Dc, float Da) LoadColor(TColor srcPixel, TColor dstPixel) {
		var tmpSrcPixel = srcPixel.Convert<TColor, T, ColorRGBA<float>, float>();
		var tmpDstPixel = dstPixel.Convert<TColor, T, ColorRGBA<float>, float>();
		var srcVec = new Vector3(tmpSrcPixel.R, tmpSrcPixel.G, tmpSrcPixel.B);
		var dstVec = new Vector3(tmpDstPixel.R, tmpDstPixel.G, tmpDstPixel.B);
		return (srcVec, tmpSrcPixel.A, dstVec, tmpDstPixel.A);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static TColor FlushColor(Vector3 Dc, float Da) => new ColorRGBA<float>(Dc.X, Dc.Y, Dc.Z, Da).Convert<ColorRGBA<float>, float, TColor, T>();

	// note: we do not pre-multiply alpha.

	// Dca' = Sca + Dca * (1 - Sa)
	// Da'  = Sa + Da - Sa * Da
	public static void OverPixel(TColor srcPixel, ref TColor dstPixel) {
		var (Sc, Sa, Dc, Da) = LoadColor(srcPixel, dstPixel);

		var DcPrime = Sc + Dc * (1 - Sa);
		var DaPrime = Sa + Da - Sa * Da;

		dstPixel = FlushColor(DcPrime, DaPrime);
	}

	// Dca' = Sca * Da
	// Da'  = Sa * Da
	public static void InPixel(TColor srcPixel, ref TColor dstPixel) {
		var (Sc, Sa, _, Da) = LoadColor(srcPixel, dstPixel);

		var DcPrime = Sc * Da;
		var DaPrime = Sa * Da;

		dstPixel = FlushColor(DcPrime, DaPrime);
	}

	// Dca' = Sca * (1 - Da)
	// Da'  = Sa * (1 - Da)
	public static void OutPixel(TColor srcPixel, ref TColor dstPixel) {
		var (Sc, Sa, _, Da) = LoadColor(srcPixel, dstPixel);

		var DcPrime = Sc * (1 - Da);
		var DaPrime = Sa * (1 - Da);

		dstPixel = FlushColor(DcPrime, DaPrime);
	}

	// Dca' = Sca * Da + Dca * (1 - Sa)
	// Da'  = Da
	public static void AtopPixel(TColor srcPixel, ref TColor dstPixel) {
		var (Sc, Sa, Dc, Da) = LoadColor(srcPixel, dstPixel);

		var DcPrime = Sc * Da + Dc * (1 - Sa);

		dstPixel = FlushColor(DcPrime, Da);
	}

	// Dca' = Sca * (1 - Da) + Dca * (1 - Sa)
	// Da'  = Sa + Da - 2 * Sa * Da
	public static void XorPixel(TColor srcPixel, ref TColor dstPixel) {
		var (Sc, Sa, Dc, Da) = LoadColor(srcPixel, dstPixel);

		var DcPrime = Sc * (1 - Da) + Dc * (1 - Sa);
		var DaPrime = Sa + Da - 2 * Sa * Da;

		dstPixel = FlushColor(DcPrime, DaPrime);
	}

	// Dca' = Sca + Dca
	//	Da' = Sa + Da
	public static void PlusPixel(TColor srcPixel, ref TColor dstPixel) {
		var (Sc, Sa, Dc, Da) = LoadColor(srcPixel, dstPixel);

		var DcPrime = Sc + Dc;
		var DaPrime = Sa + Da;

		dstPixel = FlushColor(DcPrime, DaPrime);
	}

	// Dca' = Sca * Dca + Sca * (1 - Da) + Dca * (1 - Sa)
	// Da'  = Sa + Da - Sa * Da
	public static void MultiplyPixel(TColor srcPixel, ref TColor dstPixel) {
		var (Sc, Sa, Dc, Da) = LoadColor(srcPixel, dstPixel);

		var DcPrime = Sc * Dc + Sc * (1 - Da) + Dc * (1 - Sa);
		var DaPrime = Sa + Da - Sa * Da;

		dstPixel = FlushColor(DcPrime, DaPrime);
	}

	// Dca' = Sca + Dca - Sca * Dca
	// Da'  = Sa + Da - Sa * Da
	public static void ScreenPixel(TColor srcPixel, ref TColor dstPixel) {
		var (Sc, Sa, Dc, Da) = LoadColor(srcPixel, dstPixel);

		var DcPrime = Sc * Dc + Sc * (1 - Da) + Dc * (1 - Sa);
		var DaPrime = Sa + Da - Sa * Da;

		dstPixel = FlushColor(DcPrime, DaPrime);
	}

	// if 2 * Dca <= Da
	//	Dca' = 2 * Sca * Dca + Sca * (1 - Da) + Dca * (1 - Sa)
	// otherwise
	//	Dca' = Sca * (1 + Da) + Dca * (1 + Sa) - 2 * Dca * Sca - Da * Sa
	//	Da'  = Sa + Da - Sa * Da
	public static void OverlayPixel(TColor srcPixel, ref TColor dstPixel) {
		var (Sc, Sa, Dc, Da) = LoadColor(srcPixel, dstPixel);

		var DcPrime = new Vector3(OverlayFloat(Sc.X, Dc.X, Sa, Da),
			OverlayFloat(Sc.Y, Dc.Y, Sa, Da),
			OverlayFloat(Sc.Z, Dc.Z, Sa, Da));
		var DaPrime = Sa + Da - Sa * Da;

		dstPixel = FlushColor(DcPrime, DaPrime);
	}

	public static float OverlayFloat(float Sca, float Dca, float Sa, float Da) {
		if (Math.Abs(2 * Dca - Da) <= PixelOperations.Epsilon) {
			return 2 * Sca * Dca + Sca * (1 - Da) + Dca * (1 - Sa);
		}

		return Sca * (1 + Da) + Dca * (1 + Sa) - 2 * Dca * Sca - Da * Sa;
	}

	// Dca' = min(Sca * Da, Dca * Sa) + Sca * (1 - Da) + Dca * (1 - Sa)
	// Da'  = Sa + Da - Sa * Da
	public static void DarkenPixel(TColor srcPixel, ref TColor dstPixel) {
		var (Sc, Sa, Dc, Da) = LoadColor(srcPixel, dstPixel);

		var ScDa = Sc * Da;
		var DcSa = Dc * Sa;

		var DcPrime = Vector3.Min(ScDa, DcSa) + Sc * (1 - Da) + Dc * (1 - Sa);
		var DaPrime = Sa + Da - Sa * Da;

		dstPixel = FlushColor(DcPrime, DaPrime);
	}

	// Dca' = max(Sca * Da, Dca * Sa) + Sca * (1 - Da) + Dca * (1 - Sa)
	// Da'  = Sa + Da - Sa * Da
	public static void LightenPixel(TColor srcPixel, ref TColor dstPixel) {
		var (Sc, Sa, Dc, Da) = LoadColor(srcPixel, dstPixel);

		var ScDa = Sc * Da;
		var DcSa = Dc * Sa;

		var DcPrime = Vector3.Max(ScDa, DcSa) + Sc * (1 - Da) + Dc * (1 - Sa);
		var DaPrime = Sa + Da - Sa * Da;

		dstPixel = FlushColor(DcPrime, DaPrime);
	}

	// if Sca == Sa and Dca == 0
	//	Dca' = Sca * (1 - Da)
	// otherwise if Sca == Sa
	//	Dca' = Sa * Da + Sca * (1 - Da) + Dca * (1 - Sa)
	// otherwise if Sca < Sa
	//	Dca' = Sa * Da * min(1, Dca/Da * Sa/(Sa - Sca)) + Sca * (1 - Da) + Dca * (1 - Sa)
	// Da'  = Sa + Da - Sa * Da
	public static void ColorDodgePixel(TColor srcPixel, ref TColor dstPixel) {
		var (Sc, Sa, Dc, Da) = LoadColor(srcPixel, dstPixel);

		var DcPrime = new Vector3(ColorDodgeFloat(Sc.X, Dc.X, Sa, Da),
			ColorDodgeFloat(Sc.Y, Dc.Y, Sa, Da),
			ColorDodgeFloat(Sc.Z, Dc.Z, Sa, Da));
		var DaPrime = Sa + Da - Sa * Da;

		dstPixel = FlushColor(DcPrime, DaPrime);
	}

	public static float ColorDodgeFloat(float Sca, float Dca, float Sa, float Da) {
		var isOpaque = Math.Abs(Sca - Sa) <= PixelOperations.Epsilon;

		return isOpaque switch {
			true when Math.Abs(Da) <= PixelOperations.Epsilon => Sca * (1 - Da),
			true => Sa * Da + Sca * (1 - Da) + Dca * (1 - Sa),
			_ => Sa * Da * Math.Min(1, Dca / Da * Sa / (Sa - Sca)) + Sca * (1 - Da) + Dca * (1 - Sa),
		};
	}

	// if Sca == 0 and Dca == Da
	//	Dca' = Sa * Da + Dca * (1 - Sa)
	// otherwise if Sca == 0
	//	Dca' = Dca * (1 - Sa)
	// otherwise if Sca > 0
	//	Dca' = Sa * Da * (1 - min(1, (1 - Dca/Da) * Sa/Sca)) + Sca * (1 - Da) + Dca * (1 - Sa)
	// Da'  = Sa + Da - Sa * Da
	public static void ColorBurnPixel(TColor srcPixel, ref TColor dstPixel) {
		var (Sc, Sa, Dc, Da) = LoadColor(srcPixel, dstPixel);

		var DcPrime = new Vector3(ColorBurnFloat(Sc.X, Dc.X, Sa, Da),
			ColorBurnFloat(Sc.Y, Dc.Y, Sa, Da),
			ColorBurnFloat(Sc.Z, Dc.Z, Sa, Da));
		var DaPrime = Sa + Da - Sa * Da;

		dstPixel = FlushColor(DcPrime, DaPrime);
	}

	public static float ColorBurnFloat(float Sca, float Dca, float Sa, float Da) {
		var isTransparent = Math.Abs(Sca) <= PixelOperations.Epsilon;

		return isTransparent switch {
			true when Math.Abs(Sca - Sa) <= PixelOperations.Epsilon => Sa * Da + Dca * (1 - Sa),
			true => Dca * (1 - Sa),
			_ => Sa * Da * (1 - Math.Min(1, (1 - Dca / Da) * Sa / Sca)) + Sca * (1 - Da) + Dca * (1 - Sa),
		};
	}

	// if 2 * Sca <= Sa
	//	Dca' = 2 * Sca * Dca + Sca * (1 - Da) + Dca * (1 - Sa)
	// otherwise
	//	Dca' = Sca * (1 + Da) + Dca * (1 + Sa) - Sa * Da - 2 * Sca * Dca
	// Da'  = Sa + Da - Sa * Da
	public static void HardLightPixel(TColor srcPixel, ref TColor dstPixel) {
		var (Sc, Sa, Dc, Da) = LoadColor(srcPixel, dstPixel);

		var DcPrime = new Vector3(HardLightFloat(Sc.X, Dc.X, Sa, Da),
			HardLightFloat(Sc.Y, Dc.Y, Sa, Da),
			HardLightFloat(Sc.Z, Dc.Z, Sa, Da));
		var DaPrime = Sa + Da - Sa * Da;

		dstPixel = FlushColor(DcPrime, DaPrime);
	}

	public static float HardLightFloat(float Sca, float Dca, float Sa, float Da) {
		if (Math.Abs(2 * Sca - Sa) <= PixelOperations.Epsilon) {
			return 2 * Sca * Dca + Sca * (1 - Da) + Dca * (1 - Sa);
		}

		return Sca * (1 + Da) + Dca * (1 + Sa) - Sa * Da - 2 * Sca * Dca;
	}

	// if 2 * Sca <= Sa
	//	Dca' = Dca * (Sa + (2 * Sca - Sa) * (1 - m)) + Sca * (1 - Da) + Dca * (1 - Sa)
	// otherwise if 2 * Sca > Sa and 4 * Dca <= Da
	//	Dca' = Da * (2 * Sca - Sa) * (16 * m^3 - 12 * m^2 - 3 * m) + Sca - Sca * Da + Dca
	// otherwise if 2 * Sca > Sa and 4 * Dca > Da
	// 	Dca' = Da * (2 * Sca - Sa) * (m^0.5 - m) + Sca - Sca * Da + Dca
	// Da'  = Sa + Da - Sa * Da
	// m = Dca/Da
	public static void SoftLightPixel(TColor srcPixel, ref TColor dstPixel) {
		var (Sc, Sa, Dc, Da) = LoadColor(srcPixel, dstPixel);

		var DcPrime = new Vector3(SoftLightFloat(Sc.X, Dc.X, Sa, Da),
			SoftLightFloat(Sc.Y, Dc.Y, Sa, Da),
			SoftLightFloat(Sc.Z, Dc.Z, Sa, Da));
		var DaPrime = Sa + Da - Sa * Da;

		dstPixel = FlushColor(DcPrime, DaPrime);
	}

	public static float SoftLightFloat(float Sca, float Dca, float Sa, float Da) {
		var m = Dca / Da;

		var Sc50 = Math.Abs(2 * Sca - Sa) <= PixelOperations.Epsilon;
		var Dc25 = Math.Abs(4 * Dca - Da) <= PixelOperations.Epsilon;

		return Sc50 switch {
			true => Dca * (Sa + (2 * Sca - Sa) * (1 - m)) + Sca * (1 - Da) + Dca * (1 - Sa),
			false when Dc25 => (float) (Da * (2 * Sca - Sa) * (16 * Math.Pow(m, 3) - 12 * Math.Pow(m, 2) - 3 * m) + Sca - Sca * Da + Dca),
			_ => (float) (Da * (2 * Sca - Sa) * (Math.Pow(m, 0.5) - m)) + Sca - Sca * Da + Dca,
		};
	}

	// Dca' = Sca + Dca - 2 * min(Sca * Da, Dca * Sa)
	// Da'  = Sa + Da - Sa * Da
	public static void DifferencePixel(TColor srcPixel, ref TColor dstPixel) {
		var (Sc, Sa, Dc, Da) = LoadColor(srcPixel, dstPixel);

		var DcPrime = Sc + Dc - 2 * Vector3.Min(Sc * Da, Dc * Sa);
		var DaPrime = Sa + Da - Sa * Da;

		dstPixel = FlushColor(DcPrime, DaPrime);
	}

	// Dca' = (Sca * Da + Dca * Sa - 2 * Sca * Dca) + Sca * (1 - Da) + Dca * (1 - Sa)
	// Da'  = Sa + Da - Sa * Da
	public static void ExclusionPixel(TColor srcPixel, ref TColor dstPixel) {
		var (Sc, Sa, Dc, Da) = LoadColor(srcPixel, dstPixel);

		var DcPrime = Sc * Da + Dc * Sa - 2 * Sc * Dc + Sc * (1 - Da) + Dc * (1 - Sa);
		var DaPrime = Sa + Da - Sa * Da;

		dstPixel = FlushColor(DcPrime, DaPrime);
	}

	// Dca' = Sc * Sa + Dc * (1 - Sa)
	// Da'  = max(Sa, Da)
	public static void AlphaBlendPixel(TColor srcPixel, ref TColor dstPixel) {
		var (Sc, Sa, Dc, Da) = LoadColor(srcPixel, dstPixel);

		var DcPrime = Sc * Sa + Dc * (1 - Sa);
		var DaPrime = Math.Max(Sa, Da);

		dstPixel = FlushColor(DcPrime, DaPrime);
	}
}
