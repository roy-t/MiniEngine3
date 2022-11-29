using System.Text;

namespace Mini.Engine.Content.Serialization;
internal static class PathGenerator
{
    private static readonly HashSet<char> InvalidFileNameChars = new(Path.GetInvalidFileNameChars());
    private static readonly string Extension = ".mec";

    public static string GetPath(ContentId id)
    {
        var folder = Path.GetDirectoryName(id.Path) ?? string.Empty;
        var file = Path.GetFileName(id.Path);
        if(!string.IsNullOrEmpty(id.Key))
        {
            var key = SanitizeKey(id.Key);
            file += "#" + key;
        }

        file = "." + file + Extension;
        return Path.Combine(folder, file);        
    }

    private static string SanitizeKey(string key)
    {
        var builder = new StringBuilder();
        foreach (var c in key)
        {
            if (InvalidFileNameChars.Contains(c))
            {
                builder.Append('_');
            }
            else
            {
                builder.Append(c);
            }
        }

        return builder.ToString();
    }
}
