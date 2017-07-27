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
        private ILog _log;

        public ServiceHostInstaller()
        {
            _log = Log.For(this);
        }

        public override void Install(IDictionary stateSaver)
        {
            var configurator =
                (IServiceConfigurator)CallContext.LogicalGetData(WindowsServiceInstaller.ServiceConfiguratorKey);

            if (configurator == null)
            {
                throw new InstallException("No install configuration could be located in the call context.");
            }

            var hasUserName = !string.IsNullOrEmpty(configurator.Username);
            var hasPassword = !string.IsNullOrEmpty(configurator.Password);

            if (hasUserName && !hasPassword)
            {
                throw new InstallException("A username has been specified without a password.  Please specify both or none.");
            }

            if (hasPassword && !hasUserName)
            {
                throw new InstallException("A password has been specified without a username.  Please specify both or none.");
            }

            var processInstaller = new ServiceProcessInstaller();

            if (hasUserName && hasPassword)
            {
                _log.Trace(string.Format("[ServiceAccount] : username = '{0}' with specified password", configurator.Username));

                processInstaller.Account = ServiceAccount.User;
                processInstaller.Username = configurator.Username;
                processInstaller.Password = configurator.Password;
            }
            else
            {
                _log.Trace("[ServiceAccount] : LocalSystem");

                processInstaller.Account = ServiceAccount.LocalSystem;
            }

            var installer = new ServiceInstaller
            {
                DisplayName = configurator.DisplayName,
                ServiceName = configurator.InstancedServiceName(),
                Description = configurator.Description,
                StartType = configurator.ServiceStartMode
            };

            Installers.Add(processInstaller);
            Installers.Add(installer);

            base.Install(stateSaver);
        }

        public override void Uninstall(IDictionary savedState)
        {
            var configuration =
                (IServiceConfigurator)
                    CallContext.LogicalGetData(WindowsServiceInstaller.ServiceInstallerConfigurationKey);

            if (configuration == null)
            {
                throw new InstallException("No uninstall configuration could be located in the call context.");
            }

            var processInstaller = new ServiceProcessInstaller();

            var installer = new ServiceInstaller
            {
                ServiceName = configuration.InstancedServiceName()
            };

            Installers.Add(processInstaller);
            Installers.Add(installer);

            base.Uninstall(savedState);
        }
    }
}