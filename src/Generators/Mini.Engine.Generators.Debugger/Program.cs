using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Mini.Engine.Content.Generators;
using Mini.Engine.ECS.Generators;

namespace Mini.Engine.Generators.Debugger;

partial class Program
{
    private record SourceFile(string Name, SourceText Text);

    static void Main(string[] args)
    {
        var sourceArgs = args.Where(f => Path.GetExtension(f).Equals(".cs", StringComparison.InvariantCultureIgnoreCase));
        var shaderArgs = args.Where(f => Path.GetExtension(f).Equals(".hlsl", StringComparison.InvariantCultureIgnoreCase));

        var sources = new List<GeneratedSourceResult>();
        foreach (var arg in args)
        {
            var source = File.ReadAllText(arg);
            var compilation = Compiler.CreateCompilationFromSource(source);
            
            if (Path.GetExtension(arg).Equals(".cs", StringComparison.InvariantCultureIgnoreCase))
            {
                sources.AddRange(Compiler.Test(compilation, new SystemGenerator(), args));
            }
            else if (Path.GetExtension(arg).Equals(".hlsl", StringComparison.InvariantCultureIgnoreCase))
            {
                //sources.AddRange(Compiler.Test(compilation, new ShaderGenerator(), args));
                sources.AddRange(Compiler.Test(compilation, new ShaderGenerator(), args));
            }
        }

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
