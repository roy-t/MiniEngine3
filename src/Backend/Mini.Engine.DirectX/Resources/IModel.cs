using System;

namespace Mini.Engine.DirectX.Resources;

public interface IModel : IMesh, IDisposable
{
    Primitive[] Primitives { get; }
    IMaterial[] Materials { get; }
}