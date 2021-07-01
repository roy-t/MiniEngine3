using System.IO;
using Microsoft.CodeAnalysis;

namespace Mini.Engine.Content.Generators
{
    [Generator]
    public class ShaderDataTypesGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {

        }

        public void Execute(GeneratorExecutionContext context)
        {
            foreach (var file in context.AdditionalFiles)
            {
                var text = "namespace Mini.Engine.Content { public static class Foo { public static void Bar() {} } }";

                var name = Path.GetFileName(file.Path);
                context.AddSource($"{name}.cs", text);
            }
        }
    }
}
