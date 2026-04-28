// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

namespace Sedna.GPU;

public abstract class ManagedResource<T> : IDisposable where T : struct, IResourceId {
	protected ManagedResource(T id, ResourceManager manager) {
		Id = id;
		Manager = manager;
	}

	public T Id { get; internal set; }
	public ResourceManager Manager { get; internal set; }

	public void Dispose() {
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	public abstract void Create();
	public abstract void Destroy();

	protected virtual void Dispose(bool disposing) => Manager.Destroy(Id);

	~ManagedResource() => Dispose(false);
}
