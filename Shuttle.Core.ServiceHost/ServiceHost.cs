using System;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Core.ServiceHost
{
    public sealed class ServiceHost : ServiceBase
    {
        private readonly ServiceHostEventLog _log;
        private readonly IServiceHostStart _service;

        private ServiceHost(IServiceHostStart service, ServiceConfiguration configuration)
        {
            Guard.AgainstNull(service, nameof(service));
            Guard.AgainstNull(configuration, nameof(configuration));

            _service = service;

            ServiceName = configuration.ServiceName;

            _log = new ServiceHostEventLog(ServiceName);

            AppDomain.CurrentDomain.UnhandledException += UnhandledException;
        }

        private void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = (Exception) e.ExceptionObject;

            _log.WrinteEntry(ex.Message, EventLogEntryType.Error);
        }

        protected override void OnStart(string[] args)
        {
            _log.WrinteEntry(string.Format("[starting] : service name = '{0}'", ServiceName));

            _service.Start();

            _log.WrinteEntry(string.Format("[started] : service name = '{0}'", ServiceName));
        }

        protected override void OnStop()
        {
            _log.WrinteEntry(string.Format("[stopping] : service name = '{0}'", ServiceName));

            var stoppable = _service as IServiceHostStop;

            stoppable?.Stop();

            var disposable = _service as IDisposable;

            disposable?.Dispose();

            _log.WrinteEntry(string.Format("[stopped] : service name = '{0}'", ServiceName));
        }

        public static void Run<T>() where T : IServiceHostStart, new()
        {
            Run(new T(), null);
        }

        public static void Run<T>(Action<ServiceConfiguration> configure) where T : IServiceHostStart, new ()
        {
            Run(new T(), configure);
        }

        public static void Run(IServiceHostStart service)
        {
            Run(service, null);
        }

        public static void Run(IServiceHostStart service, Action<ServiceConfiguration> configure)
        {
            Guard.AgainstNull(service, nameof(service));

            var configuration = new ServiceConfiguration();

            configure?.Invoke(configuration);

            if (new CommandProcessor().Execute(configuration))
            {
                return;
            }

            try
            {
                if (!Environment.UserInteractive)
                {
                    Run(new ServiceBase[]
                    {
                        new ServiceHost(service, configuration)
                    });
                }
                else
                {
                    Console.CursorVisible = false;

                    new ConsoleService(service, configuration).Execute();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }
    }
}