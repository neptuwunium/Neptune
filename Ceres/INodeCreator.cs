// SPDX-FileCopyrightText: 2023-2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

namespace Ceres;

public interface INodeCreator {
	// ReSharper disable once UnusedMemberInSuper.Global
	public (Node Node, int Id) CreateNode(Root root, string name);
}
