using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SharpGen.Runtime;
using Vortice.Direct3D;

namespace Mini.Engine.DirectX
{
    internal sealed class ShaderFileInclude : CallbackBase, Include
    {
        private readonly string RootFolder;
        private readonly List<IDisposable> Disposables;

        public ShaderFileInclude(string rootFolder)
        {
            this.RootFolder = rootFolder;
            this.Disposables = new List<IDisposable>();
        }

        public void Close(Stream stream)
            => stream.Close();

        public Stream Open(IncludeType type, string fileName, Stream parentStream)
        {
            // Ensure C# handles all the text handling, conversions, BOMs, etc...
            // and return the raw ASCII bytes to DirectX
            var path = type == IncludeType.Local
                ? Path.Combine(this.RootFolder, fileName)
                : fileName;

            var text = File.ReadAllText(path);
            var bytes = Encoding.ASCII.GetBytes(text);
            var stream = new MemoryStream(bytes, false);
            this.Disposables.Add(stream);

            return stream;
        }

        protected override void Dispose(bool disposing)
        {
            this.Disposables.ForEach(d => d.Dispose());
            this.Disposables.Clear();

            base.Dispose(disposing);
        }
    }
}
