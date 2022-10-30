using Mini.Engine.IO;

namespace Mini.Engine.Content.v2.Serialization;
internal static class Utilities
{
    public static bool IsCurrent(IContentProcessor manager, ContentHeader header, IVirtualFileSystem fileSystem)
    {
        if (header.Version != manager.Version)
        {
            return false;
        }

        var lastWrite = header.Dependencies
            .Select(d => fileSystem.GetLastWriteTime(d))
            .Append(header.Timestamp).Max();

        return lastWrite <= header.Timestamp;
    }
}
