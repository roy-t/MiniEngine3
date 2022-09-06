using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Mini.Engine.Content;
internal class PathComparer : IComparer<string?>, IEqualityComparer<string?>, IComparer, IEqualityComparer
{
    public static readonly PathComparer Instance = new();

    private readonly StringComparer Comparer;

    public PathComparer()
    {
        this.Comparer = StringComparer.InvariantCultureIgnoreCase;
    }    

    public int Compare(string? x, string? y)
    {
        return this.Comparer.Compare(NormalizePath(x), NormalizePath(y));
    }

    public int Compare(object? x, object? y)
    {
        return this.Comparer.Compare(x as string, y as string);
    }    

    public bool Equals(string? x, string? y)
    {
        return this.Comparer.Equals(NormalizePath(x), NormalizePath(y));
    }

    public new bool Equals(object? x, object? y)
    {
        return this.Equals(x as string, y as string);
    }

    public int GetHashCode([DisallowNull] string? obj)
    {
        return this.Comparer.GetHashCode(NormalizePath(obj));
    }

    public int GetHashCode(object obj)
    {
        return this.Comparer.GetHashCode(NormalizePath(obj as string));
    }    

    private static string NormalizePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return string.Empty;
        }

        return path
            .Trim()
            .Replace('\\', '/');
    }
}
