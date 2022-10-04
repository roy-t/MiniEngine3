using System.Numerics;
using Mini.Engine.Core;
using Xunit;
using Xunit.Sdk;
namespace Mini.Engine.Tests;
public class ScratchPad
{

    public interface ISomething { }

    public interface ISomethingSpecial : ISomething { }

    public interface IFoo<out T> where T : ISomething
    {
        public int Id { get; }
    }

    public readonly record struct Foo<T>(int Id)
        : IFoo<T>
        where T : ISomething;



    [Fact]
    public void Do()
    {
        var foo = new Foo<ISomething>(1);
        var bar = new Foo<ISomethingSpecial>(1);

        DoSomething(foo);
        DoSomething(bar);
    }

    private void DoSomething(IFoo<ISomething> item)
    {

    }

    [Fact]
    public void Randoms()
    {
        var sequence = new QuasiRandomSequence();
        var dims = new List<Vector2>();

        for(var i = 0; i < 100; i++)
        {
            var v = sequence.Next2D(-1.0f, 1.0f);
            dims.Add(v);
        }


        dims.ToString();
    }
}
