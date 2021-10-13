using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Mini.Engine.Windows;
using Vortice.Win32;
using static Mini.Engine.Input.RawInput;

namespace Mini.Engine.Input
{
    public sealed class RawMouseController
    {
        public RawMouseController(IntPtr hwnd)
        {
            Win32Application.RawEvents.OnEvent += (s, e) => this.ProcessMessage(e.Msg, e.WParam, e.LParam);

            var devices = new RawInputDevice[2];
            devices[0].UsagePage = HIDUsagePage.Generic;
            devices[0].Usage = HIDUsage.Mouse;
            devices[0].Flags = RawInputDeviceFlags.InputSink;
            devices[0].WindowHandle = hwnd;

            devices[1].UsagePage = HIDUsagePage.Generic;
            devices[1].Usage = HIDUsage.Keyboard;
            devices[1].Flags = RawInputDeviceFlags.InputSink;
            devices[1].WindowHandle = hwnd;

            var succes = RegisterRawInputDevices(devices, 1, Marshal.SizeOf<RawInputDevice>());
            if (!succes)
            {
                throw new Exception("Could not register input devices");
            }
        }

        private bool ProcessMessage(WindowMessage msg, UIntPtr wParam, IntPtr lParam)
        {
            // TODO: no keyboard input messages? :'(

            switch (msg)
            {
                case WindowMessage.Input:
                    var inputSize = Marshal.SizeOf<RawInputData>();
                    var headerSize = Marshal.SizeOf<RawInputHeader>();

                    var result = GetRawInputData(lParam, RawInputCommand.Input, out var rawInput, ref inputSize, headerSize);
                    if (result != 48)
                    {

                    }
                    // result should be >= 0!
                    if (rawInput.Header.Type == RawInputType.Mouse)
                    {
                        Debug.WriteLine($"Mouse: p:{rawInput.Data.Mouse.LastX},{rawInput.Data.Mouse.LastY}. {rawInput.Data.Mouse.ButtonFlags}, {rawInput.Data.Mouse.ButtonData / WheelDelta}");
                    }
                    else if (rawInput.Header.Type == RawInputType.Keyboard)
                    {
                        Debug.WriteLine($"Keyboard: {rawInput.Data.Keyboard.VirtualKey}, {rawInput.Data.Keyboard.MakeCode}. {rawInput.Data.Keyboard.Flags}");
                    }
                    else
                    {

                    }

                    break;
            }


            return false;
        }
    }
}
