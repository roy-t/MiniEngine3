using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Vortice.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.KeyboardAndMouseInput;
using static Windows.Win32.PInvoke;

namespace Mini.Engine.Windows
{
    public sealed class RawInputController
    {
        private const ushort HID_USAGE_PAGE_GENERIC = 0x01;
        private const ushort HID_USAGE_GENERIC_MOUSE = 0x02;
        private const ushort HID_USAGE_GENERIC_KEYBOARD = 0x06;
        private const ushort RIM_TYPEMOUSE = 0;
        private const ushort RIM_TYPEKEYBOARD = 1;

        private static readonly uint RawInputSize = (uint)Marshal.SizeOf<RAWINPUT>();
        private static readonly uint RawInputHeaderSize = (uint)Marshal.SizeOf<RAWINPUTHEADER>();

        public RawInputController(IntPtr hwnd)
        {
            Win32Application.RegisterMessageListener(WindowMessage.Input, (wParam, lParam) => this.ProcessMessage(wParam, lParam));

            var devices = new RAWINPUTDEVICE[] { CreateKeyboard(hwnd), CreateMouse(hwnd) };
            var success = RegisterRawInputDevices(devices, (uint)Marshal.SizeOf<RAWINPUTDEVICE>());
            if (!success)
            {
                throw new Exception("Could not register input devices");
            }
        }

        private unsafe void ProcessMessage(UIntPtr wParam, IntPtr lParam)
        {
            var size = RawInputSize;

            var rawInput = new RAWINPUT();
            GetRawInputData((HRAWINPUT)lParam, RAW_INPUT_DATA_COMMAND_FLAGS.RID_INPUT, &rawInput, ref size, RawInputHeaderSize);

            if (rawInput.header.dwType == RIM_TYPEMOUSE)
            {
                Debug.WriteLine($"Mouse: p:{Mouse.GetPosition(rawInput)}, {Mouse.GetButtons(rawInput)}, {Mouse.GetMouseWheel(rawInput)}");
            }

            if (rawInput.header.dwType == RIM_TYPEKEYBOARD)
            {
                Debug.WriteLine($"Keyboard: {Keyboard.GetKey(rawInput)}, {Keyboard.GetScanCode(rawInput)}, {Keyboard.GetEvent(rawInput)}");
            }
        }

        private static RAWINPUTDEVICE CreateMouse(IntPtr hwnd)
        {
            return new RAWINPUTDEVICE
            {
                usUsagePage = HID_USAGE_PAGE_GENERIC,
                usUsage = HID_USAGE_GENERIC_MOUSE,
                dwFlags = RAWINPUTDEVICE_FLAGS.RIDEV_INPUTSINK,
                hwndTarget = (HWND)hwnd
            };
        }

        private static RAWINPUTDEVICE CreateKeyboard(IntPtr hwnd)
        {
            return new RAWINPUTDEVICE
            {
                usUsagePage = HID_USAGE_PAGE_GENERIC,
                usUsage = HID_USAGE_GENERIC_KEYBOARD,
                dwFlags = RAWINPUTDEVICE_FLAGS.RIDEV_INPUTSINK,
                hwndTarget = (HWND)hwnd
            };
        }
    }
}
