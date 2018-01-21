using System;
using System.Diagnostics;
using System.ServiceProcess;
using Shuttle.Core.Contract;
using Shuttle.Core.Logging;

namespace Shuttle.Core.ServiceHost
{
    public sealed class ServiceHost : ServiceBase
    {
        private readonly ServiceHostEventLog _log;
        private readonly IServiceHostStart _service;

        private ServiceHost(IServiceHostStart service, IServiceConfiguration configuration)
        {
            Guard.AgainstNull(service, nameof(service));
            Guard.AgainstNull(configuration, nameof(configuration));

            _service = service;

            ServiceName = configuration.ServiceName;

            _log = GetServiceHostEventLog(configuration);

            AppDomain.CurrentDomain.UnhandledException += UnhandledException;
        }

        private static ServiceHostEventLog GetServiceHostEventLog(IServiceConfiguration configuration)
        {
            return new ServiceHostEventLog(configuration.ServiceName);
        }

        private void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = (Exception) e.ExceptionObject;

            _log.WrinteEntry(ex.Message, EventLogEntryType.Error);
        }

        protected override void OnStart(string[] args)
        {
            _log.WrinteEntry($"[starting] : service name = '{ServiceName}'");

            _service.Start();

            _log.WrinteEntry($"[started] : service name = '{ServiceName}'");
        }

        protected override void OnStop()
        {
            _log.WrinteEntry($"[stopping] : service name = '{ServiceName}'");

            var stoppable = _service as IServiceHostStop;

            stoppable?.Stop();

            var disposable = _service as IDisposable;

            disposable?.Dispose();

            _log.WrinteEntry($"[stopped] : service name = '{ServiceName}'");
        }

        public static void Run<T>() where T : IServiceHostStart, new()
        {
            if (CommandProcessed())
            {
                return;
            }

            Run(new T(), null);
        }

        public static void Run<T>(Action<IServiceConfiguration> configure) where T : IServiceHostStart, new()
        {
            if (CommandProcessed(configure))
            {
                return;
            }

            Run(new T(), configure);
        }

        public static void Run(IServiceHostStart service)
        {
            if (CommandProcessed())
            {
                return;
            }

            Run(service, null);
        }

        public static void Run(IServiceHostStart service, Action<IServiceConfiguration> configure)
        {
            if (CommandProcessed(configure))
            {
                return;
            }

            Guard.AgainstNull(service, nameof(service));

            var configuration = ServiceHostSection.Configuration();

            configure?.Invoke(configuration);

            if (!Environment.UserInteractive)
            {
                try
                {
                    Run(new ServiceBase[]
                    {
                        new ServiceHost(service, configuration)
                    });
                }
                catch (Exception ex)
                {
                    GetServiceHostEventLog(configuration).WrinteEntry(ex.Message, EventLogEntryType.Error);
                    throw;
                }
            }
            else
            {
                try
                {
                    Console.CursorVisible = false;

                    new ConsoleService(service, configuration).Execute();
                }
                catch (Exception ex)
                {
                    Log.For(typeof(ServiceHost)).Error(ex.Message);
                    throw;
                }
            }
        }

        private static bool CommandProcessed(Action<IServiceConfiguration> configure = null)
        {
            var configuration = new ServiceConfiguration();

            configure?.Invoke(configuration);

            return new CommandProcessor().Execute(configuration);
        }
    }
}