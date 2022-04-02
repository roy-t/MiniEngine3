namespace Mini.Engine.Content.Generators;

internal static class Utilities
{
    public static string FindPathFromMarkerFile(string path, string fileMarker)
    {
        var relativePath = Path.GetFileName(path);
        var directory = new DirectoryInfo(Path.GetDirectoryName(path));
        while (directory != null && !directory.EnumerateFiles(fileMarker).Any())
        {
            relativePath = Path.Combine(directory.Name, relativePath);
            directory = directory.Parent;

        }

        if (directory != null)
        {
            return relativePath;
        }
        else
        {
            throw new Exception($"Could not find {fileMarker} file in the directory {Path.GetDirectoryName(path)} or any of its parent directories");
        }
    }
}