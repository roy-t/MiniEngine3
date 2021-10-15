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

            if (rawInput.header.dwType == 0) // Mouse
            {
                const short WheelDelta = 120;
                Debug.WriteLine($"Mouse: p:{rawInput.data.mouse.lLastX},{rawInput.data.mouse.lLastY}. {rawInput.data.mouse.Anonymous.Anonymous.usButtonFlags}, {(short)rawInput.data.mouse.Anonymous.Anonymous.usButtonData / WheelDelta}");
            }

            if (rawInput.header.dwType == 1) // Keyboard
            {
                Debug.WriteLine($"Keyboard: {rawInput.data.keyboard.VKey}, {rawInput.data.keyboard.MakeCode}. {rawInput.data.keyboard.Flags}");
            }
        }

        private static RAWINPUTDEVICE CreateMouse(IntPtr hwnd)
        {
            return new RAWINPUTDEVICE
            {
                usUsagePage = 0x01,
                usUsage = 0x02,
                dwFlags = RAWINPUTDEVICE_FLAGS.RIDEV_INPUTSINK,
                hwndTarget = (HWND)hwnd
            };
        }

        private static RAWINPUTDEVICE CreateKeyboard(IntPtr hwnd)
        {
            return new RAWINPUTDEVICE
            {
                usUsagePage = 0x01,
                usUsage = 0x06,
                dwFlags = RAWINPUTDEVICE_FLAGS.RIDEV_INPUTSINK,
                hwndTarget = (HWND)hwnd
            };
        }
    }
}
