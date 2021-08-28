namespace Mini.Engine.IO
{
    public interface IVirtualFileSystem
    {
        string ReadAllText(string path);
        IEnumerable<string> GetChangedFiles();
        void WatchFile(string path);
        void ClearChangedFiles();
    }
}