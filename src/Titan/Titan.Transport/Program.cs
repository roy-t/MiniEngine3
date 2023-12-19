using System.Diagnostics;
using LightInject;
using Mini.Engine.Core.Lifetime;
using Mini.Engine.Debugging;
using Mini.Engine.DirectX;
using Mini.Engine.Windows;
using Serilog;
using static Windows.Win32.UI.Input.KeyboardAndMouse.VIRTUAL_KEY;

namespace Titan.Transport;

internal class Program
{
    private static readonly ushort Escape = InputService.GetScanCode(VK_ESCAPE);

    [STAThread]
    static void Main(string[] args)
    {
        var logger = InitializeLoggingSystem();
        using var services = InitializeServices(logger);

        var metrics = new MetricService();

        using var window = Win32Application.Initialize("Titan Transport");
        var (input, keyboard, mouse) = CreateInput(window);

        using var lifetime = new LifetimeManager(logger);
        lifetime.PushFrame("Initialization");

        using var device = new Device(window.Handle, window.Width, window.Height, lifetime) { VSync = false };

        RegisterServices(services, metrics, window, lifetime, device, input);
        var gameloop = services.GetInstance<GameLoop>();
        Run(metrics, window, device, input, keyboard, mouse, gameloop);

        lifetime.PopFrame("Initialization");
    }

    private static void Run(MetricService metrics, Win32Window window, Device device, InputService input, Keyboard keyboard, Mouse mouse, GameLoop gameloop)
    {
        var stopwatch = new Stopwatch();
        while (Win32Application.PumpMessages())
        {
            stopwatch.Restart();

            input.NextFrame();
            while (input.ProcessEvents(keyboard))
            {
                if (keyboard.Pressed(Escape))
                {
                    window.Dispose();
                }
            }
            device.ImmediateContext.ClearBackBuffer();
            gameloop.Step();
            device.Present();

            metrics.Update("Program.Main", (float)stopwatch.Elapsed.TotalMilliseconds);
            // metrics.UpdateBuiltInGauges();

            Resize(window, device);
        }
    }

    private static void Resize(Win32Window window, Device device)
    {
        if (window.Width != device.Width || window.Height != device.Height)
        {
            device.Resize(window.Width, window.Height);
        }
    }

    private static ILogger InitializeLoggingSystem()
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .MinimumLevel.Information()
            .WriteTo.Debug(outputTemplate: "{Level:u4} [{SourceContext}] {Message:lj}{NewLine}\t{Exception}")
            .CreateLogger();

        return Log.Logger.ForContext<Program>();
    }

    private static ServiceContainer InitializeServices(ILogger logger)
    {
        var options = new ContainerOptions()
        {
            LogFactory = (type) => logEntry =>
            {
                switch (logEntry.Level)
                {
                    case LogLevel.Warning:
                        logger.Warning(logEntry.Message);
                        break;
                    default:
                        logger.Information(logEntry.Message);
                        break;
                }
            }
        };

        var container = new ServiceContainer(options);
        container.SetDefaultLifetime<PerContainerLifetime>();

        container.RegisterInstance<IServiceContainer>(container);

        return container;
    }

    private static (InputService, Keyboard, Mouse) CreateInput(Win32Window window)
    {
        var input = new InputService(window);
        var keyboard = new Keyboard();
        var mouse = new Mouse();

        return (input, keyboard, mouse);
    }


    private static void RegisterServices(ServiceContainer services, MetricService metrics, Win32Window window, LifetimeManager lifetime, Device device, InputService input)
    {
        services.RegisterInstance(metrics);
        services.RegisterInstance(window);
        services.RegisterInstance(lifetime);
        services.RegisterInstance(device);
        services.RegisterInstance(input);
    }

}