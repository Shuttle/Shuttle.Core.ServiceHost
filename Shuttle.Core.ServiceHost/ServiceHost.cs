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
        private readonly HostServiceConfiguration _hostServiceConfiguration;
        private readonly HostEventLog _log;
        private readonly IService _service;

        private ServiceHost(IService service)
        {
            Guard.AgainstNull(service, nameof(service));

            _service = service;
            Guard.AgainstNull(hostServiceConfiguration, "hostServiceConfiguration");

            _hostServiceConfiguration = hostServiceConfiguration;

            ServiceName = hostServiceConfiguration.ServiceName;

            _log = new HostEventLog(ServiceName);

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

            var stoppable = _service as IStoppable;

            stoppable?.Stop();

            var disposable = _service as IDisposable;

            disposable?.Dispose();

            _log.WrinteEntry(string.Format("[stopped] : service name = '{0}'", ServiceName));
        }

        public void Execute<T>() where T : IService, new ()
        {
            Execute(new T());
        }

        public void Execute(IService service)
        {
            Guard.AgainstNull(service, nameof(service));

            if (new CommandProcessor().Execute())
            {
                return;
            }

            if (!Environment.UserInteractive)
            {
                Run(new ServiceBase[]
                {
                    new ServiceHost(service)
                });
            }
            else
            {
                Console.CursorVisible = false;

                new ConsoleService(service).Execute();
            }
        }
    }
}