﻿using System;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Core.ServiceHost
{
    public class ConsoleService
    {
        private readonly IServiceHostStart _service;
        private readonly ServiceConfigurator _configurator;

        public ConsoleService(IServiceHostStart service, ServiceConfigurator configurator)
        {
            Guard.AgainstNull(service, nameof(service));
            Guard.AgainstNull(configurator, nameof(configurator));

            _service = service;
            _configurator = configurator;
        }

        public void Execute()
        {
            var serviceController =
                ServiceController.GetServices()
                    .FirstOrDefault(s => s.ServiceName == _configurator.ServiceName);

            if (serviceController != null && serviceController.Status == ServiceControllerStatus.Running)
            {
                ColoredConsole.WriteLine(ConsoleColor.Yellow,
                    "WARNING: Windows service '{0}' is running.  The display name is '{1}'.",
                    _configurator.ServiceName, serviceController.DisplayName);
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

                ColoredConsole.WriteLine(ConsoleColor.Green, "[stopping]");

                waitHandle.Set();

                e.Cancel = true;
                stopping = true;
            };

            _service.Start();

            Console.WriteLine();
            ColoredConsole.WriteLine(ConsoleColor.Green, "[started] : '{0}'.",
                _configurator.ServiceName);
            Console.WriteLine();
            ColoredConsole.WriteLine(ConsoleColor.DarkYellow, "[press ctrl+c to stop]");
            Console.WriteLine();

            WaitHandle.WaitAny(waitHandles);

            var stoppable = _service as IServiceHostStop;

            stoppable?.Stop();

            var disposable = _service as IDisposable;

            disposable?.Dispose();
        }
    }
}