using System.Reflection;

using HFM.Console;
using HFM.Console.DependencyInjection;
using HFM.Core.Client;
using HFM.Core.Logging;
using HFM.Preferences;

using LightInject;
using LightInject.Microsoft.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;

var exitEvent = new ManualResetEvent(false);

Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    exitEvent.Set();
};

Application.SetPaths(
    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HFM"));

using var services = new ServiceContainer();
services.RegisterAssembly(Assembly.GetExecutingAssembly());
var provider = services.CreateServiceProvider(EmptyServiceCollection.Instance);

ConfigureLogging();
using var clientScheduledTasks = LoadClients(args[0]);

exitEvent.WaitOne();

void ConfigureLogging()
{
    var logger = (Logger)provider.GetRequiredService<ILogger>();
#if DEBUG
    logger.Level = LoggerLevel.Debug;
#else
    logger.Level = provider.GetRequiredService<IPreferences>().Get<LoggerLevel>(Preference.MessageLevel);
#endif

    var loggerEvents = provider.GetRequiredService<ILoggerEvents>();
    loggerEvents.Logged += (_, e) =>
    {
        foreach (var message in e.Messages)
        {
            Console.WriteLine(message);
        }
    };
}

ClientScheduledTasks LoadClients(string path)
{
    var tasks = provider.GetRequiredService<ClientScheduledTasks>();

    var settings = new ClientSettingsFileSerializer().Deserialize(path);
    var configuration = provider.GetRequiredService<ClientConfiguration>();
    configuration.Load(settings!);

    return tasks;
}
