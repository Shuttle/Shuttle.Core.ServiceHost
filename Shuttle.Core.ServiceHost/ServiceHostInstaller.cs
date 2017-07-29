using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.Runtime.Remoting.Messaging;
using System.ServiceProcess;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Core.ServiceHost
{
    [RunInstaller(true)]
    public class ServiceHostInstaller : Installer
    {
        private readonly ILog _log;

        public ServiceHostInstaller()
        {
            _log = Log.For(this);
        }

        public override void Install(IDictionary stateSaver)
        {
            var configuration =
                (IServiceConfiguration)CallContext.LogicalGetData(WindowsServiceInstaller.ServiceConfigurationKey);

            if (configuration == null)
            {
                throw new InstallException("No install configuration could be located in the call context.");
            }

            var hasUserName = !string.IsNullOrEmpty(configuration.Username);
            var hasPassword = !string.IsNullOrEmpty(configuration.Password);

            if (hasUserName && !hasPassword)
            {
                throw new InstallException("A username has been specified without a password.  Please specify both or none.");
            }

            if (hasPassword && !hasUserName)
            {
                throw new InstallException("A password has been specified without a username.  Please specify both or none.");
            }

            var processInstaller = new ServiceProcessInstaller();

            if (hasUserName)
            {
                _log.Trace(string.Format("[ServiceAccount] : username = '{0}' with specified password", configuration.Username));

                processInstaller.Account = ServiceAccount.User;
                processInstaller.Username = configuration.Username;
                processInstaller.Password = configuration.Password;
            }
            else
            {
                _log.Trace("[ServiceAccount] : LocalSystem");

                processInstaller.Account = ServiceAccount.LocalSystem;
            }

            var installer = new ServiceInstaller
            {
                DisplayName = configuration.DisplayName,
                ServiceName = configuration.GetInstancedServiceName(),
                Description = configuration.Description,
                StartType = configuration.StartMode
            };

            Installers.Add(processInstaller);
            Installers.Add(installer);

            base.Install(stateSaver);
        }

        public override void Uninstall(IDictionary savedState)
        {
            var configuration =
                (IServiceConfiguration)
                    CallContext.LogicalGetData(WindowsServiceInstaller.ServiceConfigurationKey);

            if (configuration == null)
            {
                throw new InstallException("No uninstall configuration could be located in the call context.");
            }

            var processInstaller = new ServiceProcessInstaller();

            var installer = new ServiceInstaller
            {
                ServiceName = configuration.GetInstancedServiceName()
            };

            Installers.Add(processInstaller);
            Installers.Add(installer);

            base.Uninstall(savedState);
        }
    }
}