namespace Mini.Engine.IO
{
    public sealed class DiskFileSystem : IVirtualFileSystem
    {
        public DiskFileSystem(string rootDirectory)
        {
            this.RootDirectory = Path.GetFullPath(rootDirectory);
        }

        public string RootDirectory { get; }

        public string ReadAllText(string path)
        {
            return File.ReadAllText(this.CombinePath(path));
        }

        private string CombinePath(string path)
        {
            if (Path.IsPathRooted(path))
            {
                throw new ArgumentException($"Expected relative path but got '{path}'", nameof(path));
            }

            return Path.Combine(this.RootDirectory, path);
        }
    }
}
