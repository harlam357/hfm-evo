﻿using HFM.Core.Logging;

using LightInject;

namespace HFM.Console.DependencyInjection;

internal sealed class LoggingCompositionRoot : ICompositionRoot
{
    public void Compose(IServiceRegistry serviceRegistry)
    {
        var logger = new FileSystemLogger(Application.DataFolderPath!);
        serviceRegistry.AddSingleton<ILogger>(_ => logger);
        serviceRegistry.AddSingleton<ILoggerEvents>(_ => logger);
    }
}
