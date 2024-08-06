using System.Reflection;
using LightInject;
using Serilog;

namespace Mini.Engine.Configuration;
public sealed class Injector : IDisposable
{
    private const string DefaultOutputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} @ {SourceContext}{NewLine}{Exception}";
    private static readonly string[] IgnoredAssemblies = new[]
    {
            "ImGui.NET", "LightInject", "Microsoft", "Mini.Engine.Content.Generators", "Mini.Engine.DirectX", "Mini.Engine.Windows", "NativeLibraryLoader", "Newtonsoft", "Serilog", "ShaderTools", "SharpGen", "Vortice", "StbImageSharp"
        };

    private readonly ServiceContainer Container;
    private readonly ComponentCatalog Components;
    private readonly ILogger Logger;

    public Injector()
    {
        Log.Logger = new LoggerConfiguration()
         .Enrich.FromLogContext()
#if DEBUG
         .MinimumLevel.Debug()
#else
         .MinimumLevel.Information()
#endif
         .WriteTo.Debug(outputTemplate: DefaultOutputTemplate)
         .CreateLogger();

        this.Logger = Log.Logger.ForContext<Injector>();

        var options = new ContainerOptions()
        {
            LogFactory = (type) => logEntry => this.Logger.Debug(logEntry.Message),
            EnableOptionalArguments = true
        };

        this.Container = new ServiceContainer(options);

        this.Registry = new ServiceRegistryAdapter(this.Container);
        this.Factory = new ServiceFactoryAdapter(this.Container);

        this.Container.SetDefaultLifetime<PerContainerLifetime>()
            .RegisterInstance(this.Registry)
            .RegisterInstance(this.Factory)
            .RegisterInstance(Log.Logger);

        var assemblies = this.LoadAssembliesInCurrentDirectory();
        this.RegisterTypesFromAssemblies(assemblies);
        this.Components = new ComponentCatalog(assemblies);

        foreach (var component in this.Components)
        {
            this.Logger.Debug("Registered component {@component}", component.FullName);
        }

        this.Logger.Debug("Registered {@count} components", this.Components.Count);

        this.Container.RegisterInstance(this.Components);
    }

    public IServiceRegistry Registry { get; }
    public IServiceFactory Factory { get; }

    public void Dispose()
    {

        this.Container?.Dispose();
    }

    private void RegisterTypesFromAssemblies(IEnumerable<Assembly> assemblies)
    {
        foreach (var assembly in assemblies)
        {
            _ = this.Container.RegisterAssembly(assembly, (serviceType, concreteType) =>
            {
                // Do not register services as an implementation of an abstract class or interface
                if (serviceType != concreteType)
                {
                    return false;
                }

                if (IsServiceType(concreteType))
                {
                    this.Logger.Debug("Registered service {@service}", concreteType.FullName);
                    return true;
                }

                if (IsContentType(concreteType))
                {
                    this.Logger.Debug("Registered content {@content}", concreteType.FullName);
                    return true;
                }

                return false;
            });
        }

        this.Logger.Information("Registered {@count} classes", this.Container.AvailableServices.Count());
    }

    private IEnumerable<Assembly> LoadAssembliesInCurrentDirectory()
    {
        var cwd = Directory.GetCurrentDirectory();
        this.Logger.Information("Loading assemblies from {@directory}", cwd);

        var assemblies = new List<Assembly>();
        foreach (var file in Directory.EnumerateFiles(cwd, "*.dll"))
        {
            try
            {
                var assemblyName = AssemblyName.GetAssemblyName(file);
                if (IsRelevantAssembly(assemblyName))
                {
                    this.Logger.Information("Loading {@assembly}", assemblyName.FullName);
                    var assembly = Assembly.LoadFrom(file);
                    assemblies.Add(assembly);
                }
                else
                {
                    this.Logger.Debug("Ignoring {@assembly} as its not a relevant assembly", assemblyName.FullName);
                }
            }
            catch (BadImageFormatException)
            {
                this.Logger.Debug("Ignoring {@file} as its not a .NET assembly", file);
            }
        }

        return assemblies;
    }

    private static bool IsServiceType(Type type)
        => type.IsDefined(typeof(ServiceAttribute), true) && !type.IsAbstract;

    private static bool IsContentType(Type type)
        => type.IsDefined(typeof(ContentAttribute), true) && !type.IsAbstract;

    private static bool IsRelevantAssembly(AssemblyName name)
        => !IgnoredAssemblies.Any(n => name.FullName.StartsWith(n, StringComparison.InvariantCultureIgnoreCase));


    public void RegisterContainer(Type containerType)
    {
        foreach (var componentType in this.Components)
        {
            this.RegisterContainerFor(containerType, componentType);
        }

        this.Logger.Information("Registered {@container} for {@count} components", containerType.FullName, this.Components.Count);
    }

    private void RegisterContainerFor(Type containerType, Type componentType)
    {
        containerType = containerType.MakeGenericType(componentType);
        var parameters = containerType
            .GetConstructors()[0]
            .GetParameters()
            .Select(p => this.Container.GetInstance(p.ParameterType)).ToArray();

        var instance = Activator.CreateInstance(containerType, parameters);

        foreach (var interfaceType in containerType.GetInterfaces())
        {
            _ = this.Container.RegisterInstance(interfaceType, instance);
        }

        _ = this.Container.RegisterInstance(containerType, instance);
    }
}
