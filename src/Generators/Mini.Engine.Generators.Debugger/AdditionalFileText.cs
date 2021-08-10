using System.IO;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Mini.Engine.Generators.Debugger
{
    internal sealed class AdditionalFileText : AdditionalText
    {
        public AdditionalFileText(string path)
        {
            this.Path = path;
        }

        public override string Path { get; }

        public override SourceText GetText(CancellationToken cancellationToken = default)
            => SourceText.From(File.ReadAllText(this.Path));
    }
}
