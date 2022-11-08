namespace Mini.Engine.DirectX.Resources.Models;

public interface IModel : IMesh, IDisposable
{
    IReadOnlyList<Primitive> Primitives { get; }
    IReadOnlyList<IMaterial> Materials { get; }
}