using System;
using System.Diagnostics;
using System.Text;
using Shuttle.Core.Contract;

namespace Shuttle.Core.ServiceHost
{
    public enum ServiceCommand
    {
        Install = 1,
        Uninstall = 2
    }

    public interface IServiceInvoker
    {
        void Execute(ServiceCommand command);
    }

    public class ServiceInvoker : IServiceInvoker
    {
        private readonly IServiceConfiguration _configuration;

        public ServiceInvoker(IServiceConfiguration configuration)
        {
            Guard.AgainstNull(configuration, nameof(configuration));

            _configuration = configuration;
        }

        public void Execute(ServiceCommand command)
        {
            Guard.AgainstNull(command, nameof(command));

            if (string.IsNullOrEmpty(_configuration.ServicePath))
            {
                throw new InvalidOperationException("No service path has been specified.");
            }

            var outputData = new StringBuilder();

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    UseShellExecute = false,
                    Arguments = string.Concat("/", command, " ", _configuration.CommandLine()),
                    FileName = _configuration.ServicePath
                },
                EnableRaisingEvents = true
            };

            process.OutputDataReceived += delegate (object sender, DataReceivedEventArgs e) { outputData.Append(e.Data); };

            process.Start();
            process.BeginOutputReadLine();
            var existed = process.WaitForExit(_configuration.Timeout);
            process.CancelOutputRead();

            if (!existed)
            {
                throw new ApplicationException($"The call to service '{_configuration.ServicePath}' timed out.  The operation result is inconclusive.");
            }

            var output = outputData.ToString();

            switch (command)
            {
                case ServiceCommand.Install:
                    {
                        if (!output.Contains("'Shuttle.Core.ServiceHost.Server' has been successfully installed"))
                        {
                            throw new ApplicationException("Could not successfully install service.");
                        }

                        break;
                    }
                case ServiceCommand.Uninstall:
                    {
                        if (!output.Contains("'Shuttle.Core.ServiceHost.Server' has been successfully uninstalled"))
                        {
                            throw new ApplicationException("Could not successfully install service.");
                        }

                        break;
                    }
            }
        }
    }
}