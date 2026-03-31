// SPDX-FileCopyrightText: 2026 Neptuwunium <ada@chronovore.dev>
//
// SPDX-License-Identifier: EUPL-1.2

using Triton;

namespace Sedna.Geometry;

public record struct Texture(string Name, IImageBuffer Image, int BindingSlot, SamplingWrap WrapX, SamplingWrap WrapY, SamplingOperation Sampling);
