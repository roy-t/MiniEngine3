namespace Mini.Engine.DirectX.Debugging;

internal static class DebugMessageProvider
{
    public static string UnterminateString(string message)
    {
        if (message.EndsWith('\0'))
        {
            return message[0..^1];
        }
        return message;
    }
}
