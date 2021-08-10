using System.Linq;
using Microsoft.CodeAnalysis;
using Mini.Engine.Generators.Source.CSharp;

namespace Mini.Engine.ECS.Generators
{
    [Generator]
    public class SystemGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new ProcessAttributeReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is ProcessAttributeReceiver receiver)
            {
                var generatedFiles = receiver.Targets.Values.Select(target =>
                {
                    return SourceFile.Build($"{target.Class.Identifier.ValueText}.Generated.cs")
                        .Using("Mini.Engine.ECS.Systems")
                        .Usings(Utilities.GetUsings(target.Class))
                        .Namespace(Utilities.GetNamespace(context.Compilation, target.Class))
                            .Class($"{target.Class.Identifier.ValueText}Binding", "public", "sealed")
                                .Inherits("ISystemBinding")
                                .Field(target.Class.Identifier.ValueText, "System", "private", "readonly")
                                    .Complete()
                                .Constructor("public")
                                    .Parameter(target.Class.Identifier.ValueText, "system")
                                    .Parameter("ContainerStore", "containerStore")
                                    // TODO: add body
                                    .Complete()
                                .Method("void", "Process", "public")
                                    // TODO: add body
                                    .Complete()
                                .Complete()
                            .Class($"{target.Class.Identifier.ValueText}", "public", "partial")
                                .Inherits("ISystemBindingProvider")
                                .Method("ISystemBinding", "Bind", "public")
                                    .Parameter("ContainerStore", "containerStore")
                                    // TODO: add body
                                    .Complete()
                                .Complete()
                            .Complete()
                        .Complete();

                });

                foreach (var file in generatedFiles)
                {
                    var writer = new SourceWriter();
                    file.Generate(writer);
                    context.AddSource(file.Name, writer.ToString());
                }
            }
        }
    }
}
