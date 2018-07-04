using System;
using System.Reflection;
#if (!NETCOREAPP2_0 && !NETCOREAPP2_1 && !NETSTANDARD2_0)
using System.Linq;
using System.ServiceProcess;
#endif
using System.Threading;
using Shuttle.Core.Contract;

namespace Shuttle.Core.ServiceHost
{
    public class ConsoleService
    {
        private readonly IServiceHostStart _service;

#if (!NETCOREAPP2_0 && !NETCOREAPP2_1 && !NETSTANDARD2_0)
        private readonly IServiceConfiguration _configuration;

        public ConsoleService(IServiceHostStart service, IServiceConfiguration configuration)
        {
            Guard.AgainstNull(service, nameof(service));
            Guard.AgainstNull(configuration, nameof(configuration));

            _configuration = configuration;
            _service = service;
        }
#else
        public ConsoleService(IServiceHostStart service)
        {
            Guard.AgainstNull(service, nameof(service));

            _service = service;
        }
#endif

        public void Execute()
        {
#if (!NETCOREAPP2_0 && !NETCOREAPP2_1 && !NETSTANDARD2_0)
            var serviceController =
                ServiceController.GetServices()
                    .FirstOrDefault(s => s.ServiceName == _configuration.ServiceName);

            if (serviceController != null && serviceController.Status == ServiceControllerStatus.Running)
            {
                ConsoleExtensions.WriteLine(ConsoleColor.Yellow,
                    $"WARNING: Windows service '{_configuration.ServiceName}' is running.  The display name is '{serviceController.DisplayName}'.");
                Console.WriteLine();
            }
#endif

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
#if (!NETCOREAPP2_0 && !NETCOREAPP2_1 && !NETSTANDARD2_0)
            ConsoleExtensions.WriteLine(ConsoleColor.Green, $"[started] : '{_configuration.ServiceName}'.");
#else
            ConsoleExtensions.WriteLine(ConsoleColor.Green, $"[started] : '{Assembly.GetEntryAssembly().FullName}'.");
#endif
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