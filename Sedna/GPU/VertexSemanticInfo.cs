// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using SDL;

namespace Sedna.GPU;

public record struct VertexSemanticInfo(int BufferIndex, VertexSemantic Semantic, int Offset, int Layer, SDL_GPUVertexElementFormat Format);
