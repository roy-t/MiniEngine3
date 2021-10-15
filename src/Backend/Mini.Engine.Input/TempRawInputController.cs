using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Mini.Engine.Windows;
using Vortice.Win32;
using static Mini.Engine.Input.RawInput;

namespace Mini.Engine.Input
{
    public sealed class TempRawInputController
    {
        private static readonly uint RawInputDataSize = (uint)Marshal.SizeOf<RawInputData>();
        private static readonly uint RawInputHeaderSize = (uint)Marshal.SizeOf<RawInputHeader>();

        public TempRawInputController(IntPtr hwnd)
        {
            Win32Application.RegisterMessageListener(WindowMessage.Input, (wParam, lParam) => this.ProcessMessage(wParam, lParam));

            var devices = new RawInputDevice[] { CreateKeyboard(hwnd), CreateMouse(hwnd) };
            var succes = RegisterRawInputDevices(devices, devices.Length, Marshal.SizeOf<RawInputDevice>());
            if (!succes)
            {
                throw new Exception("Could not register input devices");
            }
        }

        private static RawInputDevice CreateMouse(IntPtr hwnd)
        {
            return new RawInputDevice
            {
                UsagePage = HIDUsagePage.Generic,
                Usage = HIDUsage.Mouse,
                Flags = RawInputDeviceFlags.InputSink,
                WindowHandle = hwnd
            };
        }

        private static RawInputDevice CreateKeyboard(IntPtr hwnd)
        {
            return new RawInputDevice
            {
                UsagePage = HIDUsagePage.Generic,
                Usage = HIDUsage.Keyboard,
                Flags = RawInputDeviceFlags.InputSink,
                WindowHandle = hwnd
            };
        }

        private void ProcessMessage(UIntPtr wParam, IntPtr lParam)
        {
            var inputSize = RawInputDataSize;
            GetRawInputData(lParam, RawInputCommand.Input, out var rawInput, ref inputSize, RawInputHeaderSize);

            if (rawInput.Header.Type == RawInputType.Mouse)
            {
                Debug.WriteLine($"Mouse: p:{rawInput.Data.Mouse.LastX},{rawInput.Data.Mouse.LastY}. {rawInput.Data.Mouse.ButtonFlags}, {rawInput.Data.Mouse.ButtonData / WheelDelta}");
            }

            if (rawInput.Header.Type == RawInputType.Keyboard)
            {
                Debug.WriteLine($"Keyboard: {rawInput.Data.Keyboard.VirtualKey}, {rawInput.Data.Keyboard.MakeCode}. {rawInput.Data.Keyboard.Flags}");
            }
        }
    }
}
