using System.Diagnostics;
using System.Numerics;
using ImGuiNET;
using Mini.Engine.Content;
using Mini.Engine.Core.Lifetime;
using Mini.Engine.DirectX;
using Mini.Engine.IO;
using Mini.Engine.UI;
using Mini.Engine.Windows;
using Serilog;

using Shader = Mini.Engine.Content.Shaders.Generated.UserInterface;

namespace MultiplayerDemo;

public class Program
{
    private static int Instances = 0;
    private class SimulationSlot(SimulationManager simulation, bool active)
    {
        public SimulationManager Simulation { get; } = simulation;
        public bool Active { get; set; } = active;
    }

    [STAThread]
    static void Main(string[] arguments)
    {
        var logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .MinimumLevel.Information()
            .WriteTo.Debug()
            .CreateLogger();


        using var window = Win32Application.Initialize("MultiplayerDemo");
        var input = new SimpleInputService(window);

        using var lifetimeManager = new LifetimeManager(logger);
        var programFrame = lifetimeManager.PushFrame();

        using var device = new Device(window.Handle, window.Width, window.Height, lifetimeManager);

        var fileSystem = new DiskFileSystem(logger, arguments[0]);
        var content = new ContentManager(logger, device, lifetimeManager, fileSystem);
        var shader = new Shader(device, content);

        var uiContext = ImGui.CreateContext();
        var uiIO = ImGui.GetIO();
        uiIO.DisplaySize = new Vector2(window.Width, window.Height);

        var uiTextureRegistry = new UITextureRegistry();
        using var uiRenderer = new ImGuiRenderer(device, uiTextureRegistry, shader);
        var uiInputListener = new ImGuiInputEventListener(window);

        Win32Application.RegisterInputEventListener(window, uiInputListener);

        var stopwatch = new Stopwatch();

        var simulations = new List<SimulationSlot>();

        var server = new MultiPlayerServer();

        while (Win32Application.PumpMessages())
        {
            device.ImmediateContext.ClearBackBuffer();

            input.NextFrame();
            uiInputListener.Update();
            ImGui.NewFrame();

            ImGui.DockSpaceOverViewport(0, ImGui.GetMainViewport(), ImGuiDockNodeFlags.PassthruCentralNode);
            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("Simulations"))
                {
                    if (ImGui.MenuItem("Spawn SinglePlayer"))
                    {
                        var simulation = SimulationManager.CreateSinglePlayer($"SP {Instances++}");
                        simulations.Add(new SimulationSlot(simulation, true));
                    }

                    if (ImGui.MenuItem("Spawn Host"))
                    {
                        var simulation = SimulationManager.CreateHost(server, $"Host {Instances++}");
                        simulations.Add(new SimulationSlot(simulation, true));
                    }

                    if (ImGui.MenuItem("Spawn Client"))
                    {
                        var simulation = SimulationManager.CreateClient(server, $"Client {Instances++}");
                        simulations.Add(new SimulationSlot(simulation, true));
                    }

                    ImGui.Separator();

                    foreach (var slot in simulations)
                    {
                        var active = slot.Active;
                        if (ImGui.Checkbox($"{slot.Simulation.Id}", ref active))
                        {
                            slot.Active = active;
                        }
                    }

                    ImGui.EndMenu();
                }
                ImGui.EndMainMenuBar();
            }

            foreach (var slot in simulations)
            {
                if (slot.Active)
                {
                    ImGui.SetNextWindowSize(new Vector2(480, 640), ImGuiCond.FirstUseEver);
                    slot.Simulation.UpdateUserInterface();
                }
            }

            uiIO.DeltaTime = stopwatch.ElapsedMilliseconds;
            ImGui.Render();
            uiRenderer.Render(ImGui.GetDrawData());

            device.Present();

            if (!window.IsMinimized && (window.Width != device.Width || window.Height != device.Height))
            {
                device.Resize(window.Width, window.Height);
                uiIO.DisplaySize = new Vector2(window.Width, window.Height);
            }

            stopwatch.Restart();

            if (input.Keyboard.Pressed(VirtualKeyCode.VK_ESCAPE))
            {
                window.Dispose();
            }
        }

        lifetimeManager.PopFrame(programFrame);
    }
}

