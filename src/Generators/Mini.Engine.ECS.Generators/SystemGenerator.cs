using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Mini.Engine.Generators.Source.CSharp;

namespace Mini.Engine.ECS.Generators
{
    [Generator]
    public class SystemGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
            => context.RegisterForSyntaxNotifications(() => new ProcessAttributeReceiver());

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is ProcessAttributeReceiver receiver)
            {
                var generatedFiles = receiver.Classes
                    .Select(target => new Class(context.Compilation, target))
                    .Select(target =>
                        SourceFile.Build($"{target.Name}.Generated.cs")
                        .Using("Mini.Engine.ECS.Systems")
                        .Using("Mini.Engine.ECS.Components")
                        .Usings(target.Usings)
                        .Namespace(target.Namespace)
                            .Class($"{target.Name}Binding", "public", "sealed")
                                .Inherits("ISystemBinding")
                                .Field(target.Name, "System", "private", "readonly")
                                    .Complete()
                                .Fields(target.GetUniqueComponents()
                                    .Select(c => new Field($"IComponentContainer<{c}>", $"{c}Container", "private", "readonly")))
                                .Constructor("public")
                                    .Parameter(target.Name, "system")
                                    .Parameter("ContainerStore", "containerStore")
                                    .Body()
                                        .TextCodeBlock("this.System = system;")
                                        .TextCodeBlocks(target.GetUniqueComponents()
                                            .Select(c => $"this.{c}Container = containerStore.GetContainer<{c}>();"))
                                        .Complete()
                                    .Complete()
                                .Method("void", "Process", "public")
                                    .Body()
                                        .TextCodeBlock("this.System.OnSet();")
                                        .CodeBlocks(target.Methods.Select(m => CreateProcessBlock(m)))
                                        .TextCodeBlock("this.System.OnUnSet();")
                                        .Complete()
                                    .Complete()
                                .Complete()
                            .Class($"{target.Name}", "public", "partial")
                                .Inherits("ISystemBindingProvider")
                                .Method("ISystemBinding", "Bind", "public")
                                    .Parameter("ContainerStore", "containerStore")
                                    .Body()
                                        .TextCodeBlock($"return new {target.Name}Binding(this, containerStore);")
                                        .Complete()
                                    .Complete()
                                .Complete()
                            .Complete()
                        .Complete());

                foreach (var file in generatedFiles)
                {
                    var writer = new SourceWriter();
                    file.Generate(writer);
                    context.AddSource(file.Name, writer.ToString());
                }
            }
        }

        private static ICodeBlock CreateProcessBlock(Method method)
        {
            if (method.Query == Shared.ProcessQuery.None)
            {
                return new TextCodeBlock($"this.System.{method.Name}();");
            }

            var components = method.Components;
            if (components.Count == 0)
            {
                throw new InvalidOperationException($"Method {method.Name} with 0 arguments should not use any other ProcessQuery than 'None'");
            }

            var primary = components.First();
            var loop = new ForeachLoop("p0", $"this.{primary}Container.Get{method.Query}Items()");
            var block = new TextCodeBlock();

            for (var i = 1; i < components.Count; i++)
            {
                var component = components[i];
                block.Text.WriteLine($"if (!this.{component}Container.Contains(p0.Entity)) {{ continue; }}");
                block.Text.WriteLine($"var p{i} = this.{component}Container.Get(p0.Entity);");
            }

            var argumentList = string.Join(", ", Enumerable.Range(0, components.Count).Select(i => $"p{i}"));
            block.Text.WriteLine($"this.System.{method.Name}({argumentList});");

            var body = new Body();
            body.Code.Add(block);
            loop.Body = body;
            return loop;
        }
    }
}
