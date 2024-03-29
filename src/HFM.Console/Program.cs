﻿using System.ComponentModel;
using System.Globalization;
using System.Reflection;

using HFM.Console;
using HFM.Console.DependencyInjection;
using HFM.Console.ViewModels;
using HFM.Core.Client;
using HFM.Core.Logging;
using HFM.Preferences;

using LightInject;
using LightInject.Microsoft.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;

using var exitEvent = new ManualResetEvent(false);

Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    // ReSharper disable once AccessToDisposedClosure
    exitEvent.Set();
};

Application.SetPaths(
    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HFM"));

using var services = new ServiceContainer();
services.RegisterAssembly(Assembly.GetExecutingAssembly());
var provider = services.CreateServiceProvider(EmptyServiceCollection.Instance);

var preferences = InitializePreferences();
var logger = ConfigureLogging();
using var clientScheduledTasks = LoadClients(args[0]);

exitEvent.WaitOne();

logger.Info("----------");
logger.Info("Exiting...");
logger.Info(String.Empty);

IPreferences InitializePreferences()
{
    var p = provider.GetRequiredService<IPreferences>();
    p.Load();
    return p;
}

ILogger ConfigureLogging()
{
    var l = (FileSystemLogger)provider.GetRequiredService<ILogger>();
#if DEBUG
    l.Level = LoggerLevel.Debug;
#else
    l.Level = provider.GetRequiredService<IPreferences>().Get<LoggerLevel>(Preference.MessageLevel);
#endif
    l.Initialize();

    var loggerEvents = provider.GetRequiredService<ILoggerEvents>();
    loggerEvents.Logged += (_, e) =>
    {
        foreach (var message in e.Messages)
        {
            Console.WriteLine(message);
        }
    };

    // write log header
    l.Info(String.Empty);
    l.Info(String.Format(CultureInfo.InvariantCulture, "Starting - {0} v{1}", Application.Name, Application.Version));
    l.Info(String.Empty);

    return l;
}

ClientScheduledTasks LoadClients(string path)
{
    var tasks = provider.GetRequiredService<ClientScheduledTasks>();

    var settings = new ClientSettingsFileSerializer().Deserialize(path);
    var configuration = provider.GetRequiredService<ClientConfiguration>();
    configuration.ClientConfigurationChanged += (_, _) =>
    {
        logger.Info(ClientResourceViewModel.HeaderString());

        var comparer = new ClientResourceSortComparer
        {
            OfflineStatusLast = preferences.Get<bool>(Preference.OfflineLast)
        };
        var property = TypeDescriptor.GetProperties(typeof(ClientResource))
            .OfType<PropertyDescriptor>()
            .FirstOrDefault(x => x.Name == nameof(ClientResource.ClientIdentifier));
        comparer.SetSortProperties(property!, ListSortDirection.Ascending);

        var resources = configuration
            .SelectMany(x => x.Resources ?? new[] { ClientResource.Offline(x.Settings!) })
            .OrderBy(x => x, comparer);

        foreach (var resource in resources)
        {
            var viewModel = new ClientResourceViewModel(resource, preferences);
            logger.Info(viewModel.ToString());
        }
    };
    configuration.Load(settings!);

    return tasks;
}
