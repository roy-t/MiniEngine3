using System.ComponentModel;
using System.Numerics;
using LightInject;
using Mini.Engine.Core;
using Xunit;
using Xunit.Sdk;
namespace Mini.Engine.Tests;
public class ScratchPad
{

    public class Compo : ICompositionRoot
    {
        public void Compose(IServiceRegistry serviceRegistry)
        {            
            serviceRegistry.Register<GeneratedShader>(factory =>
            {
                var cm = factory.GetInstance<ContentManager>();
                return new GeneratedShader(cm.B);
            });
        }
    }

    public class ContentManager
    {
        public string B => "foo";
    }


    public class GeneratedShader
    {
        public GeneratedShader(string a)
        {
            this.A = a;
        }

        public string A { get; }
    }


    [Fact]
    public void Foo()
    {
        var container = new ServiceContainer();
        container.RegisterFrom<Compo>();
        container.Register<ContentManager>();
        
        var foo = container.GetInstance<GeneratedShader>();
        Assert.Equal("foo", foo.A);
    }
}
