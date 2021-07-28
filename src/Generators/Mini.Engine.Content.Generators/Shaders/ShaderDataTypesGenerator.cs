using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Mini.Engine.Content.Generators.Source;
using ShaderTools.CodeAnalysis.Hlsl;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Hlsl.Text;
using ShaderTools.CodeAnalysis.Text;


namespace Mini.Engine.Content.Generators.Shaders
{

    [Generator]
    public class ShaderDataTypesGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {

        }

        public void Execute(GeneratorExecutionContext context)
        {
            var fileSystem = new ContentFileSystem();
            foreach (var effectFile in context.AdditionalFiles
                .Where(f => System.IO.Path.GetExtension(f.Path).Equals(".fx", StringComparison.InvariantCultureIgnoreCase)))
            {
                // Parse shader data
                var contents = effectFile.GetText();
                var options = new HlslParseOptions();

                // Add the current directory for relative includes
                options.AdditionalIncludeDirectories.Add(System.IO.Path.GetDirectoryName(effectFile.Path));
                var syntaxTree = SyntaxFactory.ParseSyntaxTree(new SourceFile(contents), options, fileSystem);

                // Create the skeleton for the file, namespace and class
                var name = System.IO.Path.GetFileNameWithoutExtension(effectFile.Path);

                var file = new File($"{name}.cs");
                file.Usings.Add(new Using("Mini.Engine.DirectX"));
                file.Usings.Add(new Using("System.Runtime.InteropServices"));

                var @namespace = new Namespace("Mini.Engine.Content");
                file.Namespaces.Add(@namespace);

                var @class = new Class(name, "public", "sealed");
                @namespace.Types.Add(@class);

                @class.InheritsFrom.Add("Shader");

                var constructor = new Constructor(@class.Name, "public");
                @class.Constructors.Add(constructor);

                constructor.Parameters.Add(new Field("Device", "device"));
                constructor.Chain = new BaseConstructorCall("device", SourceUtilities.ToLiteral(effectFile.Path));

                var customTypes = Structure.FindAll(syntaxTree.Root);
                foreach (var customType in customTypes)
                {
                    var @struct = new Struct(Utilities.ToDotNetImportantName(customType.Name), "public");
                    @struct.Attributes.Add(new Source.Attribute("StructLayout", new ArgumentList("LayoutKind.Sequential")));
                    foreach (var variable in customType.Variables)
                    {
                        var typeName = TypeTranslator.TranslateToDotNet(variable);
                        @struct.Properties.Add(new Property(typeName, Utilities.ToDotNetImportantName(variable.Name), false, "public"));
                    }

                    @class.InnerTypes.Add(@struct);
                }

                var cbuffers = CBuffer.FindAll(syntaxTree.Root);
                foreach (var cbuffer in cbuffers)
                {
                    // Create a custom type for the CBuffer's data
                    var @struct = new Struct($"CBuffer{cbuffer.Slot}", "public");
                    @struct.Attributes.Add(new Source.Attribute("StructLayout", new ArgumentList("LayoutKind.Sequential")));
                    @struct.Fields.Add(new Field("int", $"Slot = {cbuffer.Slot}", "public", "const"));

                    foreach (var variable in cbuffer.Variables)
                    {
                        var typeName = TypeTranslator.TranslateToDotNet(variable);
                        @struct.Properties.Add(new Property(typeName, variable.Name, false, "public"));
                    }

                    @class.InnerTypes.Add(@struct);
                }


                var writer = new SourceWriter();
                file.Generate(writer);
                context.AddSource($"{name}.cs", writer.ToString());
            }
        }
    }
}
