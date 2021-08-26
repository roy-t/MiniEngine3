namespace Mini.Engine.IO
{
    public interface IVirtualFileSystem
    {
        string ReadAllText(string path);
    }
}