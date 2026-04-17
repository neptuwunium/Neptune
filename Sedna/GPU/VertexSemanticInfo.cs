// SPDX-FileCopyrightText: 2026 Neptuwunium <ada@chronovore.dev>
//
// SPDX-License-Identifier: EUPL-1.2

namespace Sedna.GPU;

public record struct VertexSemanticInfo(int BufferIndex, VertexSemantic Semantic, int Offset, int Layer, int ComponentCount, ComponentType ComponentType, bool Normalized);
