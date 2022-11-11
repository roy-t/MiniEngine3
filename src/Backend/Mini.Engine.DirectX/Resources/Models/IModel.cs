using Mini.Engine.Core.Lifetime;

namespace Mini.Engine.DirectX.Resources.Models;

public interface IModel : IMesh, IDisposable
{
    IReadOnlyList<Primitive> Primitives { get; }
    IReadOnlyList<ILifetime<IMaterial>> Materials { get; }
}