namespace Mini.Engine.DirectX.Resources.Models;

public interface IModel : IMesh, IDisposable
{
    Primitive[] Primitives { get; }
    IMaterial[] Materials { get; }
}