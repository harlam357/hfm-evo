using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HFM.Core.Logging;
using LightInject;

using ProtoBuf.Meta;

namespace HFM.Console.DependencyInjection;

internal class LoggingCompositionRoot : ICompositionRoot
{
    public void Compose(IServiceRegistry serviceRegistry)
    {
        var logger = new FileSystemLogger(Application.DataFolderPath!);
        serviceRegistry.AddSingleton<ILogger>(_ => logger);
        serviceRegistry.AddSingleton<ILoggerEvents>(_ => logger);
    }
}
