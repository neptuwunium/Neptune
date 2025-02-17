// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

namespace Triton;

// https://www.w3.org/TR/SVGCompositing/
public enum PixelOperation {
	Copy,
	AlphaBlend,
	SrcOver,
	DstOver,
	SrcIn,
	DstIn,
	SrcOut,
	DstOut,
	SrcAtop,
	DstAtop,
	Xor,
	Plus,
	Multiply,
	Screen,
	Overlay,
	Darken,
	Lighten,
	ColorDodge,
	ColorBurn,
	HardLight,
	SoftLight,
	Difference,
	Exclusion,
}
