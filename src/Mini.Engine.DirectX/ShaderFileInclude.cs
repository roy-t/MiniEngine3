using System.IO;
using SharpGen.Runtime;
using Vortice.Direct3D;

namespace Mini.Engine.DirectX
{
    internal sealed class ShaderFileInclude : CallbackBase, Include
    {
        private readonly string RootFolder;

        public ShaderFileInclude(string rootFolder)
        {
            this.RootFolder = rootFolder;
        }

        public void Close(Stream stream)
            => stream.Close();

        public Stream Open(IncludeType type, string fileName, Stream parentStream)
        {
            switch (type)
            {
                case IncludeType.Local:
                    return File.OpenRead(Path.Combine(this.RootFolder, fileName));
                case IncludeType.System:
                default:
                    return File.OpenRead(fileName);
            }
        }
    }
}
