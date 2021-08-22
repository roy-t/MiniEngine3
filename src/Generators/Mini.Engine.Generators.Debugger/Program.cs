using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Mini.Engine.Content.Generators;
using Mini.Engine.ECS.Generators;

namespace Mini.Engine.Generators.Debugger
{
    partial class Program
    {
        private record SourceFile(string Name, SourceText Text);

        static void Main(string[] args)
        {
            var sourceArgs = args.Where(f => Path.GetExtension(f).Equals(".cs", StringComparison.InvariantCultureIgnoreCase));
            var shaderArgs = args.Where(f => Path.GetExtension(f).Equals(".hlsl", StringComparison.InvariantCultureIgnoreCase));            

            ISourceGenerator generator = sourceArgs.Any()
                ? new SystemGenerator()
                : new ShaderGenerator();

            IEnumerable<string> fileArgs = sourceArgs.Any()
                ? sourceArgs
                : shaderArgs;

            var compilation = Compiler.CreateCompilationFromSource(File.ReadAllText(fileArgs.First()));

            var sources = Compiler.Test(compilation, generator, fileArgs);
            foreach (var source in sources)
            {
                Console.WriteLine("/// <generated>");
                Console.WriteLine($"/// {source.HintName}");
                Console.WriteLine("/// </generated>");
                Console.WriteLine(source.SourceText);

                Console.WriteLine();
            }
        }
    }
}
