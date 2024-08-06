using System.Drawing;

namespace Mini.Engine;

public static class StartupArguments
{
    public static bool EnableRenderDoc => IsPresent("--renderdoc");

    public static string ContentRoot => GetArgumentValue("--content");

    public static string GameLoopType => GetArgumentValueOrDefault("--gameloop", "Mini.Engine.GameLoop"); //  --position 3841,16,1270,1415

    public static bool EnableVSync => IsPresent("--vsync");

    public static Rectangle? WindowPosition => GetRectangle("--position");

    public static bool LaunchDebugger => IsPresent("--debugger");

    public static string Executable => Environment.GetCommandLineArgs()[0];

    private static bool IsPresent(string argument)
    {
        var args = Environment.GetCommandLineArgs();
        return args.Any(a => a.Equals(argument, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsNotPresent(string argument)
    {
        return !IsPresent(argument);
    }

    private static string GetArgumentValueOrDefault(string argument, string @default)
    {
        var value = GetArgumentValue(argument);
        if (string.IsNullOrEmpty(value))
        {
            return @default;
        }

        return value;
    }

    private static string GetArgumentValue(string argument)
    {
        var args = Environment.GetCommandLineArgs();
        for (var i = 0; i < args.Length - 1; i++)
        {
            if (args[i].Equals(argument, StringComparison.OrdinalIgnoreCase))
            {
                return Unquote(args[i + 1]);
            }
        }

        return string.Empty;
    }

    private static Rectangle? GetRectangle(string argument)
    {
        var arg = GetArgumentValue(argument);
        var elements = arg.Split(',');
        if (elements.Length == 4)
        {
            var x = int.Parse(elements[0]);
            var y = int.Parse(elements[1]);
            var w = int.Parse(elements[2]);
            var h = int.Parse(elements[3]);

            return new Rectangle(x, y, w, h);
        }

        return null;
    }

    private static string Unquote(string value)
    {
        if (value.StartsWith('"') && value.EndsWith('"'))
        {
            return value[1..^1];
        }

        return value;
    }
}
