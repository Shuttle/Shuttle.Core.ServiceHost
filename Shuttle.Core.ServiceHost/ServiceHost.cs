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

        private ServiceHost(IServiceHostStart service, ServiceConfigurator configurator)
        {
            Guard.AgainstNull(service, nameof(service));
            Guard.AgainstNull(configurator, nameof(configurator));

            _service = service;

            ServiceName = configurator.ServiceName;

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

        public void Execute<T>() where T : IServiceHostStart, new()
        {
            Execute(new T(), null);
        }

        public void Execute<T>(Action<ServiceConfigurator> configure) where T : IServiceHostStart, new ()
        {
            Execute(new T(), configure);
        }

        public void Execute(IServiceHostStart service)
        {
            Execute(service, null);
        }

        public void Execute(IServiceHostStart service, Action<ServiceConfigurator> configure)
        {
            Guard.AgainstNull(service, nameof(service));

            var configurator = new ServiceConfigurator();

            configure?.Invoke(configurator);

            if (new CommandProcessor().Execute(configurator))
            {
                return;
            }

            try
            {
                if (!Environment.UserInteractive)
                {
                    Run(new ServiceBase[]
                    {
                        new ServiceHost(service, configurator)
                    });
                }
                else
                {
                    Console.CursorVisible = false;

                    new ConsoleService(service, configurator).Execute();
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