using System.Collections.Generic;
using System.Diagnostics;

namespace Mini.Engine.Content;

public sealed class ContentId : IEquatable<ContentId>
{
    private const char Separator = '#';

    [DebuggerStepThrough]
    public ContentId(string path, string key = "")
    {
        this.Path = path;
        this.Key = key;
    }

    public string Path { get; set; }
    public string Key { get; set; }

    public static ContentId Parse(string? id)
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

    public ContentId RelativeTo(string path, string key = "")
    {
        var directory = System.IO.Path.GetDirectoryName(this.Path) ?? string.Empty;
        var fullPath = System.IO.Path.Combine(directory, path);
        return new ContentId(fullPath, key);
    }

    public override string ToString()
    {
        if (string.IsNullOrEmpty(this.Key))
        {
            return this.Path;
        }

        return $"{this.Path}{Separator}{this.Key}";
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(this.Path, this.Key);
    }

    public override bool Equals(object? obj)
    {
        return this.Equals(obj as ContentId);
    }

    public bool Equals(ContentId? other)
    {
        if (object.ReferenceEquals(this, other))
        {
            return true;
        }

        return other != null &&
            string.Equals(other.Path, this.Path, StringComparison.InvariantCultureIgnoreCase) &&
            string.Equals(other.Key, this.Key, StringComparison.InvariantCultureIgnoreCase);
    }

    public static bool operator !=(ContentId? first, ContentId? second)
    {
        return !(first == second);
    }

    public static bool operator ==(ContentId? first, ContentId? second)
    {
        if (first is null || second is null)
        {
            return ReferenceEquals(first, second);
        }

        return first.Equals(second);
    }
}