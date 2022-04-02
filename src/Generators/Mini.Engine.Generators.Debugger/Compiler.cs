using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Mini.Engine.Generators.Debugger;

public static class Compiler
{
    public static IReadOnlyList<GeneratedSourceResult> Test(Compilation compilation, IIncrementalGenerator generator, IEnumerable<string> additionalFiles)
    {
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        return RunTest(compilation, additionalFiles, driver);
    }    

    public static IReadOnlyList<GeneratedSourceResult> Test(Compilation compilation, ISourceGenerator generator, IEnumerable<string> additionalFiles)
    {
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        return RunTest(compilation, additionalFiles, driver);
    }

    private static IReadOnlyList<GeneratedSourceResult> RunTest(Compilation compilation, IEnumerable<string> additionalFiles, GeneratorDriver driver)
    {
        var array = ImmutableArray.Create
                (
                    additionalFiles
                        .Select(f => new AdditionalFileText(f))
                        .Cast<AdditionalText>()
                        .ToArray()
                );

        driver = driver.AddAdditionalTexts(array);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        var runResult = driver.GetRunResult();
        return runResult.Results.SelectMany(x => x.GeneratedSources).ToList();
    }

    public static Compilation CreateCompilationFromSource(string source)
    {
        return CSharpCompilation.Create("compilation",
                   new[] { CSharpSyntaxTree.ParseText(source) },
                   new[] { MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location) },
                   new CSharpCompilationOptions(OutputKind.ConsoleApplication));
    }
}

