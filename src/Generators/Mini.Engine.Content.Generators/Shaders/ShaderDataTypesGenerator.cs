using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Mini.Engine.Content.Generators.Source;
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
            foreach (var effectFile in context.AdditionalFiles
                .Where(f => System.IO.Path.GetExtension(f.Path).Equals(".fx", StringComparison.InvariantCultureIgnoreCase)))
            {
                var contents = effectFile.GetText();
                var syntaxTree = SyntaxFactory.ParseSyntaxTree(new SourceFile(contents), null, new ContentFileSystem());

                var name = System.IO.Path.GetFileNameWithoutExtension(effectFile.Path);

                var file = new File($"{name}.cs");
                file.Usings.Add(new Using("Mini.Engine.DirectX"));
                file.Usings.Add(new Using("System.Runtime.InteropServices"));

                var @namespace = new Namespace("Mini.Engine.Content");
                file.Namespaces.Add(@namespace);

                var @class = new Class(name, "public", "sealed");
                @namespace.Types.Add(@class);

                @class.InheritsFrom.Add("Shader");

                var literal = SourceUtilities.ToMultilineLiteral(contents.ToString());
                var contentField = new Field("string", "Code", "private static readonly");
                contentField.Value = literal;

                @class.Fields.Add(contentField);

                var constructor = new Constructor(@class.Name, "public");
                @class.Constructors.Add(constructor);

                constructor.Parameters.Add(new Field("Device", "device"));
                constructor.Chain = new BaseConstructorCall("device", SourceUtilities.ToLiteral(effectFile.Path), "Code");

                // TODO: find all custom types and add them to the namespace

                var setMethod = new Method("void", "Set", "public");
                @class.Methods.Add(setMethod);

                // TODO: body of set method!

                var cbuffers = CBuffer.FindAll(syntaxTree.Root);
                foreach (var cbuffer in cbuffers)
                {
                    var @struct = new Struct($"{name}CBuffer{cbuffer.Slot}", "public");
                    @namespace.Types.Add(@struct);

                    @struct.Attributes.Add(new Source.Attribute("StructLayout", new ArgumentList("LayoutKind.Sequential")));

                    var createMethod = new Method(@struct.Name, $"Create{@struct.Name}", "public");
                    @class.Methods.Add(createMethod);

                    createMethod.Body.Expressions.Add(new Statement($"return new ConstantBuffer<{@struct.Name}>(this.Device)"));

                    foreach (var variable in cbuffer.Variables)
                    {
                        // TODO translate HLSL type to C# type
                        @struct.Properties.Add(new Property(variable.Type, variable.Name, false, "public"));
                    }

                    setMethod.Parameters.Add(new Field($"DeviceContext", "context"));
                    setMethod.Parameters.Add(new Field($"{@struct.Name}", SourceUtilities.LowerCaseFirstLetter(@struct.Name)));
                    setMethod.Body.Expressions.Add(new Statement($"context.VS.SetConstantBuffer({cbuffer.Slot},{SourceUtilities.LowerCaseFirstLetter(@struct.Name)})"));
                    setMethod.Body.Expressions.Add(new Statement($"base.Set(context)"));
                }


                var writer = new SourceWriter();
                file.Generate(writer);
                context.AddSource($"{name}.cs", writer.ToString());
            }
        }
    }
}
