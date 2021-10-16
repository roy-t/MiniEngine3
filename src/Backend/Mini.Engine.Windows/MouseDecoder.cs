using System;
using System.Numerics;
using Windows.Win32.UI.KeyboardAndMouseInput;

namespace Mini.Engine.Windows
{
    /// <summary>
    /// Enumeration containing the button data for raw mouse input.
    /// </summary>
    [Flags()]
    internal enum ButtonFlags : ushort
    {
        /// <summary>No button.</summary>
        None = 0,
        /// <summary>Left (button 1) down.</summary>
        LeftDown = 0x0001,
        /// <summary>Left (button 1) up.</summary>
        LeftUp = 0x0002,
        /// <summary>Right (button 2) down.</summary>
        RightDown = 0x0004,
        /// <summary>Right (button 2) up.</summary>
        RightUp = 0x0008,
        /// <summary>Middle (button 3) down.</summary>
        MiddleDown = 0x0010,
        /// <summary>Middle (button 3) up.</summary>
        MiddleUp = 0x0020,
        /// <summary>Button 4 down.</summary>
        Button4Down = 0x0040,
        /// <summary>Button 4 up.</summary>
        Button4Up = 0x0080,
        /// <summary>Button 5 down.</summary>
        Button5Down = 0x0100,
        /// <summary>Button 5 up.</summary>
        Button5Up = 0x0200,
        /// <summary>Mouse wheel moved.</summary>
        MouseWheel = 0x0400,
        /// <summary>Horizontal mouse wheel moved.</summary>
        MouseHWheel = 0x0800
    }

    /// <summary>
    /// Enumeration containing the flags for raw mouse data.
    /// </summary>
    [Flags()]
    internal enum MouseFlags : ushort
    {
        /// <summary>Relative to the last position.</summary>
        MoveRelative = 0,
        /// <summary>Absolute positioning.</summary>
        MoveAbsolute = 1,
        /// <summary>Coordinate data is mapped to a virtual desktop.</summary>
        VirtualDesktop = 2,
        /// <summary>Attributes for the mouse have changed.</summary>
        AttributesChanged = 4,
        /// <summary> This mouse movement event was not coalesced. Mouse movement events can be coalesced by default. Windows XP/2000: This value is not supported.</summary>
        NoCoalesce = 8
    }

    internal static class MouseDecoder
    {
        private const short WheelDelta = 120;

        public static MouseFlags GetFlags(RAWINPUT input)
        {
            return (MouseFlags)input.data.mouse.usFlags;
        }

        public static ButtonFlags GetButtons(RAWINPUT input)
        {
            return (ButtonFlags)input.data.mouse.Anonymous.Anonymous.usButtonFlags;
        }

        public static int GetMouseWheel(RAWINPUT input)
        {
            return (short)input.data.mouse.Anonymous.Anonymous.usButtonData / WheelDelta;
        }

        public static Vector2 GetPosition(RAWINPUT input)
        {
            return new Vector2(input.data.mouse.lLastX, input.data.mouse.lLastY);
        }
    }
}
