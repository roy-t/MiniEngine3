using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Input;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using static Windows.Win32.PInvoke;

namespace Mini.Engine.Windows;

// TODO: it is quite hard to use this class right, as clicks should be handled once iteration at a time
// as to not miss the first down/up event. But helds should be handled only once otherwise you run into duplication.

public sealed class InputService
{
    private readonly record struct RawInputEvent(RAWINPUT Input, bool HasFocus);

    private const ushort HID_USAGE_PAGE_GENERIC = 0x01;
    private const ushort HID_USAGE_GENERIC_MOUSE = 0x02;
    private const ushort HID_USAGE_GENERIC_KEYBOARD = 0x06;
    private const ushort RIM_TYPEMOUSE = 0;
    private const ushort RIM_TYPEKEYBOARD = 1;

    private static readonly uint RawInputSize = (uint)Marshal.SizeOf<RAWINPUT>();
    private static readonly uint RawInputHeaderSize = (uint)Marshal.SizeOf<RAWINPUTHEADER>();

    private readonly ConcurrentQueue<RawInputEvent> EventQueue;
    private readonly List<RawInputEvent> MouseEvents;
    private readonly List<RawInputEvent> KeyboardEvents;
    private readonly Win32Window Window;

    public InputService(Win32Window window)
    {
        Win32Application.RegisterMessageListener(WM_INPUT, this.ProcessMessage);

        var devices = new RAWINPUTDEVICE[] { CreateKeyboard(window.Handle), CreateMouse(window.Handle) };
        var success = RegisterRawInputDevices(devices, (uint)Marshal.SizeOf<RAWINPUTDEVICE>());
        if (!success)
        {
            throw new Exception("Could not register input devices");
        }

        this.EventQueue = new ConcurrentQueue<RawInputEvent>();
        this.MouseEvents = new List<RawInputEvent>(3);
        this.KeyboardEvents = new List<RawInputEvent>(3);
        this.Window = window;
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
            if (input.Input.header.dwType == RIM_TYPEMOUSE)
            {
                this.MouseEvents.Add(input);
            }

            if (input.Input.header.dwType == RIM_TYPEKEYBOARD)
            {
                this.KeyboardEvents.Add(input);
            }
        }
    }

    public static ushort GetScanCode(VIRTUAL_KEY virtualKey)
    {
        return (ushort)MapVirtualKey((uint)virtualKey, MAPVK_VK_TO_VSC);
    }

    private unsafe void ProcessMessage(UIntPtr _, IntPtr lParam)
    {
        var size = RawInputSize;
        var rawInput = new RAWINPUT();
        GetRawInputData((HRAWINPUT)lParam, RAW_INPUT_DATA_COMMAND_FLAGS.RID_INPUT, &rawInput, ref size, RawInputHeaderSize);

        this.EventQueue.Enqueue(new RawInputEvent(rawInput, this.Window.HasFocus));
    }

    private static bool ProcesEvents(InputDevice device, IReadOnlyList<RawInputEvent> events)
    {
        if (device.iterator == -1)
        {
            device.NextFrame();
            device.iterator = 0;            
        }

        if (device.iterator < events.Count)
        {
            var input = events[device.iterator];
            device.NextEvent(input.Input, input.HasFocus);
            device.iterator++;
            return true;
        }

        device.iterator = -1;
        return false;
    }

    private static RAWINPUTDEVICE CreateMouse(HWND hwnd)
    {
        return new RAWINPUTDEVICE
        {
            usUsagePage = HID_USAGE_PAGE_GENERIC,
            usUsage = HID_USAGE_GENERIC_MOUSE,
            dwFlags = RAWINPUTDEVICE_FLAGS.RIDEV_INPUTSINK,
            hwndTarget = hwnd
        };
    }

    private static RAWINPUTDEVICE CreateKeyboard(HWND hwnd)
    {
        return new RAWINPUTDEVICE
        {
            usUsagePage = HID_USAGE_PAGE_GENERIC,
            usUsage = HID_USAGE_GENERIC_KEYBOARD,
            dwFlags = RAWINPUTDEVICE_FLAGS.RIDEV_INPUTSINK,
            hwndTarget = hwnd
        };
    }
}
