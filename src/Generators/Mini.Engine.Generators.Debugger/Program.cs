using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Mini.Engine.Content.Generators;

namespace Mini.Engine.Generators.Debugger
{
    class Program
    {
        private record SourceFile(string Name, SourceText Text);

        static void Main(string[] args)
        {
            Console.WriteLine("Use View -> Other Windows -> Shader Syntax Visualizer to help create the right code!!");

            var context = CreateContext();

            var generator = new ShaderDataTypesGenerator();
            generator.Execute(context);

            var files = GetGeneratedSourceFiles(context);

            foreach (var file in files)
            {
                Console.WriteLine(new string('=', 20));
                Console.WriteLine($"{file.Name}");
                Console.WriteLine(new string('=', 20));
                Console.WriteLine(file.Text);
                Console.WriteLine(new string('=', 20));

                Console.WriteLine();
            }

            Console.ReadLine();
        }

        private static GeneratorExecutionContext CreateContext()
        {
            var nonPublicInstance = BindingFlags.NonPublic | BindingFlags.Instance;

            var assembly = typeof(GeneratorExecutionContext).Assembly;
            var collectionType = assembly.GetType("Microsoft.CodeAnalysis.AdditionalSourcesCollection");
            var collection = Activator.CreateInstance(collectionType, nonPublicInstance, null, new object[] { ".cs" }, null);

            var type = typeof(GeneratorExecutionContext);
            var constructor = type.GetConstructor(nonPublicInstance, null,
                new Type[]
                {
                    typeof(Compilation),
                    typeof(ParseOptions),
                    typeof(ImmutableArray<AdditionalText>),
                    typeof(AnalyzerConfigOptionsProvider),
                    typeof(ISyntaxContextReceiver),
                    collectionType,
                    typeof(CancellationToken)
                }, null);

            var context = constructor.Invoke(new object[]
                {
                    null,
                    null,
                    null,
                    null,
                    null,
                    collection,
                    default(CancellationToken)
                });
            return (GeneratorExecutionContext)context;
        }

        private static IReadOnlyList<SourceFile> GetGeneratedSourceFiles(GeneratorExecutionContext context)
        {
            var nonPublicInstance = BindingFlags.NonPublic | BindingFlags.Instance;

            var type = typeof(GeneratorExecutionContext);
            var field = type.GetField("_additionalSources", nonPublicInstance);
            var collection = field.GetValue(context);

            var collectionType = collection.GetType();
            var method = collectionType.GetMethod("ToImmutableAndFree", nonPublicInstance);

            var array = (IEnumerable)method.Invoke(collection, null);

            var list = new List<SourceFile>();
            foreach (var value in array)
            {
                var valueType = value.GetType();
                var textProperty = valueType.GetProperty("Text");
                var nameProperty = valueType.GetProperty("HintName");

                var text = (SourceText)textProperty.GetValue(value);
                var name = (string)nameProperty.GetValue(value);

                list.Add(new SourceFile(name, text));
            }

            return list;
        }
    }
}
