using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Mini.Engine.Generators.Source.CSharp;

namespace Mini.Engine.Content.Generators
{
    public static class ContentManager
    {
        public static void AddLoadMethods(GeneratorExecutionContext context, IEnumerable<SourceFile> files, string @namespace)
        {
            var contentFile = SourceFile.Build($"{@namespace}.ContentManager.cs")
                .Using(@namespace)
                .Namespace("Mini.Engine.Content")
                    .Class("ContentManager", "public", "sealed", "partial")
                        .Methods(files
                            .SelectMany(file => file.Namespaces)
                            .SelectMany(n => n.Types)
                            .Select(type => Method.Builder(type.Name, $"Load{type.Name}", "public")
                                .Body()
                                    .TextCodeBlock($"var content = new {type.Name}(this.Device);")
                                    .TextCodeBlock($"this.Add(content);")
                                    .TextCodeBlock($"return content;")
                                    .Complete()
                                .Complete()))
                    .Complete()
                .Complete()
            .Complete();

            var writer = new SourceWriter();
            contentFile.Generate(writer);
            context.AddSource(contentFile.Name, writer.ToString());
        }
    }
}
