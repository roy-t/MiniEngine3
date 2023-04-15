#if DEBUG


[assembly: System.Reflection.Metadata.MetadataUpdateHandler(typeof(Mini.Engine.HotReloadManager))]

namespace Mini.Engine;

/// <summary>
/// Automatically called when any C# code is hot reloaded
/// </summary>
public static class HotReloadManager
{
    private static readonly HashSet<string> ChangedTypes = new();

    public static IEnumerable<string> GetChangedTypes()
    {
        if (ChangedTypes.Count == 0)
        {
            return Enumerable.Empty<string>();
        }

        var value = new List<string>(ChangedTypes);
        ChangedTypes.Clear();

        return value;
    }

    public static void ClearCache(Type[]? updatedTypes)
    {

    }

    public static void UpdateApplication(Type[]? updatedTypes)
    {
        if (updatedTypes != null)
        {
            foreach (var type in updatedTypes)
            {
                ChangedTypes.Add(type.Name);
            }
        }
    }
}

#endif
