using System;
using Windows.Win32.UI.KeyboardAndMouseInput;

namespace Mini.Engine.Windows;

[Flags]
internal enum KeyFlags : ushort
{
    Make = 0,
    Break = 1,
    E0 = 2,
    E1 = 4
}

internal static class KeyboardDecoder
{
    public static KeyFlags GetEvent(RAWINPUT input)
    {
        return (KeyFlags)input.data.keyboard.Flags;
    }

    public static VIRTUAL_KEY GetKey(RAWINPUT input)
    {
        return (VIRTUAL_KEY)input.data.keyboard.VKey;
    }

    public static ushort GetScanCode(RAWINPUT input)
    {
        return input.data.keyboard.MakeCode;
    }
}
