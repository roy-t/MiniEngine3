using Microsoft.CodeAnalysis;

namespace Mini.Engine.Content.Generators
{
    [Generator]
    public class SourceGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {

        }

        public void Execute(GeneratorExecutionContext context)
        {
            var text = "namespace Mini.Engine.Content { public static class Foo { public static void Bar() {} } }";
            context.AddSource("Foo.cs", text);
        }
    }
}
