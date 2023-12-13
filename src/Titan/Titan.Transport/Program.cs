using LightInject;
using Serilog;

namespace Titan.Transport;

internal class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        ConfigureLogging();
        var logger = CreateLogger();
        logger.Information("Hello World!");

        var container = CreateContainer(logger);

        var self = container.GetInstance<IServiceContainer>();
    }

    private static void ConfigureLogging()
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .MinimumLevel.Information()
            .WriteTo.Debug(outputTemplate: "{Level:u4} [{SourceContext}] {Message:lj}{NewLine}\t{Exception}")
            .CreateLogger();
    }

    private static ILogger CreateLogger()
    {
        return Log.Logger.ForContext<Program>();
    }

    private static ServiceContainer CreateContainer(ILogger logger)
    {
        var options = new ContainerOptions()
        {
            LogFactory = (type) => logEntry => {
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
}