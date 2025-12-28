using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ODataConsoleApp.Application.Services;
using ODataConsoleApp.Core.Configuration;
using ODataConsoleApp.Infrastructure.Repositories;
using ODataConsoleApp.Infrastructure.Search;
using ODataConsoleApp.Interfaces;
using ODataConsoleApp.UI;
using Serilog;

namespace ODataConsoleApp;

public class ApplicationHost
{
    private readonly IConfiguration _configuration;
    private IServiceProvider? _serviceProvider;

    public ApplicationHost()
    {
        try
        {
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var basePath = Path.GetDirectoryName(assemblyLocation) ?? Directory.GetCurrentDirectory();

            _configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .Build();
        }
        catch (FileNotFoundException ex)
        {
            throw new FileNotFoundException(
                $"Configuration file 'appsettings.json' was not found. " +
                $"Please ensure the file exists in the application directory: {Directory.GetCurrentDirectory()}",
                "appsettings.json",
                ex);
        }
        catch (Exception ex) when (ex is not FileNotFoundException)
        {
            throw new InvalidOperationException(
                "Failed to load application configuration. Please check that 'appsettings.json' is valid JSON.",
                ex);
        }
    }

    public async Task<int> RunAsync()
    {
        try
        {
            ConfigureSerilog();

            var services = new ServiceCollection();
            RegisterServices(services, _configuration);
            _serviceProvider = services.BuildServiceProvider();

            var appSettings = _serviceProvider.GetRequiredService<AppSettings>();
            var validator = _serviceProvider.GetRequiredService<ConfigurationValidator>();
            var validationResult = validator.Validate(appSettings);

            if (!validationResult.IsValid)
            {
                Log.Error("Configuration validation failed: {Errors}", string.Join(", ", validationResult.Errors));
                return 1;
            }

            Log.Information("Application started successfully");

            var peopleExplorerHandler = _serviceProvider.GetRequiredService<IPeopleExplorerHandler>();
            await peopleExplorerHandler.RunAsync();

            Log.Information("Application exiting normally");
            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application failed to start");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private void ConfigureSerilog()
    {
        EnsureLogsDirectoryExists("logs");

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(_configuration)
            .CreateLogger();

        Log.Information("Serilog configured successfully");
    }
    public static void EnsureLogsDirectoryExists(string logsPath)
    {
        if (!Directory.Exists(logsPath))
        {
            Directory.CreateDirectory(logsPath);
        }
    }

    public static void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        var appSettings = new AppSettings();
        configuration.Bind(appSettings);
        services.AddSingleton(appSettings);
        services.AddSingleton<IOptions<ODataServiceSettings>>(
            new OptionsWrapper<ODataServiceSettings>(appSettings.ODataService));

        services.AddSingleton<ConfigurationValidator>();

        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog();
        });

        services.AddHttpClient<IODataRepository, ODataRepository>(client =>
        {
            client.BaseAddress = new Uri(appSettings.ODataService.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(appSettings.ODataService.RequestTimeout);
        });

        services.AddSingleton<SearchStrategyFactory>();
        services.AddSingleton<IFilterStrategy, CompositeFilterStrategy>();

        services.AddSingleton<IDisplayService, DisplayService>();
        services.AddTransient<IPersonService, PersonService>();

        services.AddSingleton<IConsoleWrapper, ConsoleWrapper>();
        services.AddTransient<IPersonDetailsHandler, PersonDetailsHandler>();
        services.AddTransient<IPeopleExplorerHandler, PeopleExplorerHandler>();
        services.AddTransient<IGlobalSearchHandler, GlobalSearchHandler>();
        services.AddTransient<IAdvancedFilterHandler, AdvancedFilterHandler>();
    }
}