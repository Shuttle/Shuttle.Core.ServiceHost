using System;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Core.ServiceHost
{
    public class ConsoleService
    {
        private readonly IService _service;

        public ConsoleService(IService service)
        {
            Guard.AgainstNull(service, nameof(service));

            _service = service;
        }

        public void Execute()
        {
            var serviceController =
                ServiceController.GetServices()
                    .FirstOrDefault(s => s.ServiceName == hostServiceConfiguration.ServiceName);

            if (serviceController != null && serviceController.Status == ServiceControllerStatus.Running)
            {
                ColoredConsole.WriteLine(ConsoleColor.Yellow,
                    "WARNING: Windows service '{0}' is running.  The display name is '{1}'.",
                    hostServiceConfiguration.ServiceName, serviceController.DisplayName);
                Console.WriteLine();
            }

            var waitHandle = new ManualResetEvent(false);
            var waitHandles = new WaitHandle[] { waitHandle };

            Console.CancelKeyPress += (sender, e) =>
            {
                if (!_runServiceException)
                    ColoredConsole.WriteLine(ConsoleColor.Green, "[stopping]");
                else
                    ColoredConsole.WriteLine(ConsoleColor.DarkYellow,
                        "[press any key to close (ctrl+c does not work)]");

                waitHandle.Set();

                e.Cancel = true;
            };

            _service.Start();

            Console.WriteLine();
            ColoredConsole.WriteLine(ConsoleColor.Green, "[started] : '{0}'.",
                hostServiceConfiguration.ServiceName);
            Console.WriteLine();
            ColoredConsole.WriteLine(ConsoleColor.DarkYellow, "[press ctrl+c to stop]");
            Console.WriteLine();

            WaitHandle.WaitAny(waitHandles);

            var stoppable = _service as IStoppable;

            stoppable?.Stop();

            var disposable = _service as IDisposable;

            disposable?.Dispose();
        }
    }
}