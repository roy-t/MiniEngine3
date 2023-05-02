#if DEBUG

[assembly: System.Reflection.Metadata.MetadataUpdateHandler(typeof(Mini.Engine.HotReloadManager))]

namespace Mini.Engine;

/// <summary>
/// Automatically called when any C# code is hot reloaded
/// </summary>
public static class HotReloadManager
{
    private static readonly HashSet<string> ChangedTypes = new();
    private static readonly List<Action<string>> Reporters = new();
    private static readonly List<(string? Filter, Action<string> Callback)> Callbacks = new();

    public static void ClearCache(Type[]? updatedTypes)
    {
        ChangedTypes.Clear();
    }

    public static void UpdateApplication(Type[]? updatedTypes)
    {
        if (updatedTypes != null)
        {
            foreach (var type in updatedTypes)
            {
                var name = type.FullName ?? type.Name;
                ChangedTypes.Add(name);
                Reporters.ForEach(r => r(name));
                Callbacks.ForEach(t =>
                {
                    if (t.Filter?.StartsWith(name, StringComparison.OrdinalIgnoreCase) ?? true)
                    {
                        t.Callback(name);
                    }
                });
            }
        }
    }

    public static void AddReloadReporter(Action<string> callback)
    {
        Reporters.Add(callback);
    }

    public static void AddReloadCallback(string? filter, Action<string> callback)
    {
        Callbacks.Add((filter, callback));
    }
}

#endif
