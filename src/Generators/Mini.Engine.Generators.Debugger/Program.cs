using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.Text;
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

            var compilation = Compiler.CreateCompilationFromSource(File.ReadAllText(sourceArgs.First()));
            //var generator = new ShaderGenerator();
            var generator = new SystemGenerator();
            var sources = Compiler.Test(compilation, generator, shaderArgs);
            foreach (var source in sources)
            {
                Console.WriteLine("/// <generated>");
                Console.WriteLine($"/// {source.HintName}");
                Console.WriteLine("/// </generated>");
                Console.WriteLine(source.SourceText);

                Console.WriteLine();
            }

            Console.ReadLine();
        }
    }
}
