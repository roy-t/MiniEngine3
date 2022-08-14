using System.Numerics;
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
}
