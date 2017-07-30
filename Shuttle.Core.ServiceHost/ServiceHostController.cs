using System;
using System.ServiceProcess;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Core.ServiceHost
{
    public class ServiceHostController
    {
        private readonly IServiceConfiguration _configuration;

        public ServiceHostController(IServiceConfiguration configuration)
        {
            Guard.AgainstNull(configuration,  nameof(configuration));

            _configuration = configuration;
        }

        public ServiceHostController Start()
        {
            var service = new ServiceController(_configuration.GetInstancedServiceName());

            if (service.Status != ServiceControllerStatus.Running)
            {
                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromMilliseconds(_configuration.Timeout));
            }

            return this;
        }

        public ServiceHostController Stop()
        {
            var service = new ServiceController(_configuration.GetInstancedServiceName());

            if (service.Status != ServiceControllerStatus.Stopped)
            {
                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromMilliseconds(_configuration.Timeout));
            }

            return this;
        }
    }
}