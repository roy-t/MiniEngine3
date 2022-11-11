using Mini.Engine.Content.Serialization;
using Mini.Engine.IO;

namespace Mini.Engine.Content;

public static class ContentProcessorValidation
{
    public static void ThrowOnInvalidHeader(Guid expectedType, int expectedVersion, ContentHeader actual)
    {
        if (expectedType != actual.Type)
        {
            throw new NotSupportedException($"Unexpected type, expected: {expectedType}, actual: {actual.Type}");
        }

        if (expectedVersion != actual.Version)
        {
            throw new NotSupportedException($"Unexpected version, expected: {expectedVersion}, actual: {actual.Version}");
        }
    }

    public static bool IsContentUpToDate(int expectedVersion, ContentHeader header, IVirtualFileSystem fileSystem)
    {
        if (header.Version != expectedVersion)
        {
            return false;
        }

        var lastWrite = header.Dependencies
            .Select(d => fileSystem.GetLastWriteTime(d))
            .Append(header.Timestamp).Max();

        return lastWrite <= header.Timestamp;
    }
}
