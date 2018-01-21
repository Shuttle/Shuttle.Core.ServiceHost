using System;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using Shuttle.Core.Contract;

namespace Shuttle.Core.ServiceHost
{
    public class ConsoleService
    {
        private readonly IServiceHostStart _service;
        private readonly IServiceConfiguration _configuration;

        public ConsoleService(IServiceHostStart service, IServiceConfiguration configuration)
        {
            Guard.AgainstNull(service, nameof(service));
            Guard.AgainstNull(configuration, nameof(configuration));

            _service = service;
            _configuration = configuration;
        }

        public void Execute()
        {
            var serviceController =
                ServiceController.GetServices()
                    .FirstOrDefault(s => s.ServiceName == _configuration.ServiceName);

            if (serviceController != null && serviceController.Status == ServiceControllerStatus.Running)
            {
                ConsoleExtensions.WriteLine(ConsoleColor.Yellow,
                    $"WARNING: Windows service '{_configuration.ServiceName}' is running.  The display name is '{serviceController.DisplayName}'.");
                Console.WriteLine();
            }

            var waitHandle = new ManualResetEvent(false);
            var waitHandles = new WaitHandle[] { waitHandle };
            var stopping = false;

            Console.CancelKeyPress += (sender, e) =>
            {
                if (stopping)
                {
                    return;
                }

                ConsoleExtensions.WriteLine(ConsoleColor.Green, "[stopping]");

                waitHandle.Set();

                e.Cancel = true;
                stopping = true;
            };

            _service.Start();

            Console.WriteLine();
            ConsoleExtensions.WriteLine(ConsoleColor.Green, $"[started] : '{_configuration.ServiceName}'.");
            Console.WriteLine();
            ConsoleExtensions.WriteLine(ConsoleColor.DarkYellow, "[press ctrl+c to stop]");
            Console.WriteLine();

            WaitHandle.WaitAny(waitHandles);

            var stoppable = _service as IServiceHostStop;

            stoppable?.Stop();

            var disposable = _service as IDisposable;

            disposable?.Dispose();
        }
    }
}