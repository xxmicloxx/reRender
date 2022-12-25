using System;

namespace ReRender.Graph;

public abstract class ResourceInstance
{
    public abstract void Dispose();
}

public abstract class ResourceInstance<T> : ResourceInstance, IDisposable where T : ResourceType
{
    public abstract T ResourceType { get; }
}