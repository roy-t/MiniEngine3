using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Vortice.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.KeyboardAndMouseInput;
using static Windows.Win32.Constants;
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

        private readonly ConcurrentQueue<RAWINPUT> EventQueue;
        private readonly List<RAWINPUT> MouseEvents;
        private readonly List<RAWINPUT> KeyboardEvents;

        public RawInputController(IntPtr hwnd)
        {
            Win32Application.RegisterMessageListener(WindowMessage.Input, (wParam, lParam) => this.ProcessMessage(wParam, lParam));

            var devices = new RAWINPUTDEVICE[] { CreateKeyboard(hwnd), CreateMouse(hwnd) };
            var success = RegisterRawInputDevices(devices, (uint)Marshal.SizeOf<RAWINPUTDEVICE>());
            if (!success)
            {
                throw new Exception("Could not register input devices");
            }

            this.EventQueue = new ConcurrentQueue<RAWINPUT>();
            this.MouseEvents = new List<RAWINPUT>(3);
            this.KeyboardEvents = new List<RAWINPUT>(3);
        }

        public bool ProcessEvents(Mouse mouse)
        {
            return ProcesEvents(mouse, this.MouseEvents);
        }

        public bool ProcessEvents(Keyboard keyboard)
        {
            return ProcesEvents(keyboard, this.KeyboardEvents);
        }

        public void NextFrame()
        {
            this.MouseEvents.Clear();
            this.KeyboardEvents.Clear();

            while (this.EventQueue.TryDequeue(out var input))
            {
                if (input.header.dwType == RIM_TYPEMOUSE)
                {
                    this.MouseEvents.Add(input);
                }

                if (input.header.dwType == RIM_TYPEKEYBOARD)
                {
                    this.KeyboardEvents.Add(input);
                }
            }
        }

        public static ushort GetScanCode(VK virtualKey)
        {
            return (ushort)MapVirtualKey((uint)virtualKey, MAPVK_VK_TO_VSC);
        }

        private unsafe void ProcessMessage(UIntPtr wParam, IntPtr lParam)
        {
            var size = RawInputSize;
            var rawInput = new RAWINPUT();
            GetRawInputData((HRAWINPUT)lParam, RAW_INPUT_DATA_COMMAND_FLAGS.RID_INPUT, &rawInput, ref size, RawInputHeaderSize);

            this.EventQueue.Enqueue(rawInput);
        }

        private static bool ProcesEvents(InputDevice device, IReadOnlyList<RAWINPUT> events)
        {
            if (device.iterator == -1)
            {
                device.NextFrame();
                device.iterator = 0;
                return true;
            }

            if (device.iterator < events.Count)
            {
                device.NextEvent(events[device.iterator]);
                device.iterator++;
                return true;
            }
            else
            {
                device.iterator = -1;
                return false;
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
