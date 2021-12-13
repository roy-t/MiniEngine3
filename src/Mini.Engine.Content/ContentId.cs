using System;
using System.Linq;

namespace Mini.Engine.Content;

internal sealed class ContentId
{
    private const char Separator = '#';

    public ContentId(string path, string key = "")
    {
        this.Path = path;
        this.Key = key;
    }

    public string Path { get; set; }
    public string Key { get; set; }

    public static ContentId Parse(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Empty content id");
        }

        var separators = id.Count(c => c == Separator);
        switch (separators)
        {
            case 0:
                return new ContentId(id);
            case 1:
                var parts = id.Split(Separator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                return new ContentId(parts[0], parts[1]);
            default:
                throw new ArgumentException($"String {id} is an invalid content id as it should have the character '{Separator}' at most once ");
        }
    }
}
