using System;
using System.Collections;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Security;
using System.Security.Principal;
using Microsoft.Win32;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Core.ServiceHost
{
    public class WindowsServiceInstaller : MarshalByRefObject
    {
        public static readonly string ServiceConfiguratorKey = "__ServiceConfiguratorKey__";

        public void Install(IServiceConfigurator configurator)
        {
            Guard.AgainstNull(configurator, nameof(configurator));

            GuardAdministrator();

            if (!string.IsNullOrEmpty(configurator.ServiceAssemblyPath))
            {
                InstallRemoteService(configurator);

                return;
            }

            var log = ServiceHostEventLog.GetEventLog(configurator.InstancedServiceName());

            CallContext.LogicalSetData(ServiceConfiguratorKey, configurator);

            ColoredConsole.WriteLine(ConsoleColor.Green, "Installing service '{0}'.", configurator.InstancedServiceName());

            var assemblyInstaller = new AssemblyInstaller(typeof(ServiceHost).Assembly, null);

            using (var installer = assemblyInstaller)
            {
                IDictionary state = new Hashtable();

                installer.UseNewContext = true;

                try
                {
                    installer.Install(state);
                    installer.Commit(state);

                    var serviceKey = GetServiceKey(configurator.InstancedServiceName());

                    serviceKey.SetValue("Description", configurator.Description);
                    serviceKey.SetValue("ImagePath",
                        string.Format("{0} /serviceName:\"{1}\"{2}",
                            serviceKey.GetValue("ImagePath"),
                            configurator.ServiceName,
                            string.IsNullOrEmpty(configurator.Instance)
                                ? string.Empty
                                : string.Format(" /instance:\"{0}\"", configurator.Instance)));
                }
                catch (Exception ex)
                {
                    try
                    {
                        installer.Rollback(state);
                    }
                    catch (InstallException installException)
                    {
                        ColoredConsole.WriteLine(ConsoleColor.DarkYellow, installException.Message);
                    }

                    log.WriteEntry(ex.Message, EventLogEntryType.Error);

                    throw;
                }

                var message = string.Format("Service '{0}' has been successfully installed.", configurator.InstancedServiceName());

                log.WriteEntry(message);

                ColoredConsole.WriteLine(ConsoleColor.Green, message);
            }
        }

        private void GuardAdministrator()
        {
            var windowsIdentity = WindowsIdentity.GetCurrent();

            if (windowsIdentity == null)
            {
                throw new SecurityException(
                    "Could not get the current Windows identity.  Cannot determine if the identity is an administrator.");
            }

            var securityIdentifier = windowsIdentity.Owner;

            if (securityIdentifier == null)
            {
                throw new SecurityException(
                    "Could not get the current Windows identity's security identifier.  Cannot determine if the identity is an administrator.");
            }

            if (securityIdentifier.IsWellKnown(WellKnownSidType.BuiltinAdministratorsSid))
            {
                return;
            }

            throw new SecurityException(
                string.Format(
                    "Windows identity '{0}' is not an administrator.  Administrator privilege is required to install or uninstall a service.",
                    windowsIdentity.Name));
        }

        private void InstallRemoteService(IServiceConfigurator configurator)
        {
            var domain = RemoteDomain(configurator.ServiceAssemblyPath);

            try
            {
                var installer =
                    (WindowsServiceInstaller)
                        domain.CreateInstanceFromAndUnwrap(configurator.ServiceAssemblyPath,
                            typeof(WindowsServiceInstaller).FullName);

                throw new NotImplementedException();
                //var configuration =
                //    (InstallConfiguration)
                //        domain.CreateInstanceFromAndUnwrap(configurator.ServiceAssemblyPath,
                //            typeof(InstallConfiguration).FullName);

                //configuration.ConfigurationFileName = configurator.ConfigurationFileName;
                //configuration.Description = configurator.Description;
                //configuration.DisplayName = configurator.DisplayName;
                //configuration.StartManually = configurator.StartManually;
                //configuration.Username = configurator.Username;
                //configuration.Password = configurator.Password;
                //configuration.HostTypeAssemblyQualifiedName = configurator.HostTypeAssemblyQualifiedName;
                //configuration.Instance = configurator.Instance;
                //configuration.ServiceName = configurator.ServiceName;

                //installer.Install(configuration);
            }
            finally
            {
                AppDomain.Unload(domain);
            }
        }

        private static AppDomain RemoteDomain(string serviceAssemblyPath)
        {
            if (!File.Exists(serviceAssemblyPath))
            {
                throw new ApplicationException(string.Format("Service assembly path '{0}' does not exist.",
                    serviceAssemblyPath));
            }

            var setup = new AppDomainSetup
            {
                ApplicationBase = Path.GetDirectoryName(serviceAssemblyPath)
            };

            return AppDomain.CreateDomain("installer", AppDomain.CurrentDomain.Evidence, setup);
        }

        private void UninstallRemoteService(IServiceConfigurator configurator)
        {
            var domain = RemoteDomain(configurator.ServiceAssemblyPath);

            try
            {
                var installer =
                    (WindowsServiceInstaller)
                        domain.CreateInstanceFromAndUnwrap(configurator.ServiceAssemblyPath,
                            typeof(WindowsServiceInstaller).FullName);

                throw new NotImplementedException();
                //var configuration =
                //    (ServiceInstallerConfiguration)
                //        domain.CreateInstanceFromAndUnwrap(configurator.ServiceAssemblyPath,
                //            typeof(ServiceInstallerConfiguration).FullName);

                //configuration.HostTypeAssemblyQualifiedName =
                //    configurator.HostTypeAssemblyQualifiedName;
                //configuration.Instance = configurator.Instance;
                //configuration.ServiceName = configurator.ServiceName;

                //installer.Uninstall(configuration);
            }
            finally
            {
                AppDomain.Unload(domain);
            }
        }

        public void Uninstall(IServiceConfigurator configurator)
        {
            GuardAdministrator();

            Guard.AgainstNull(configurator, "configurator");

            if (!string.IsNullOrEmpty(configurator.ServiceAssemblyPath))
            {
                UninstallRemoteService(configurator);

                return;
            }

            var log = ServiceHostEventLog.GetEventLog(configurator.InstancedServiceName());

            CallContext.LogicalSetData(ServiceInstallerConfigurationKey, configurator);

            ColoredConsole.WriteLine(ConsoleColor.Green, "Uninstalling service '{0}'.", configurator.InstancedServiceName());

            using (var installer = new AssemblyInstaller(typeof(ServiceHost).Assembly, null))
            {
                IDictionary state = new Hashtable();

                installer.UseNewContext = true;

                try
                {
                    installer.Uninstall(state);
                }
                catch (Exception ex)
                {
                    try
                    {
                        installer.Rollback(state);
                    }
                    catch (InstallException installException)
                    {
                        ColoredConsole.WriteLine(ConsoleColor.DarkYellow, installException.Message);
                    }

                    log.WriteEntry(ex.Message, EventLogEntryType.Error);

                    throw;
                }
            }

            var message = string.Format("Service '{0}' has been successfully uninstalled.", configurator.InstancedServiceName());

            log.WriteEntry(message);

            ColoredConsole.WriteLine(ConsoleColor.Green, message);
        }

        public RegistryKey GetServiceKey(string serviceName)
        {
            var system = Registry.LocalMachine.OpenSubKey("System");
            var currentControlSet = system?.OpenSubKey("CurrentControlSet");
            var services = currentControlSet?.OpenSubKey("Services");
            var service = services?.OpenSubKey(serviceName, true);

            if (service != null)
            {
                return service;
            }

            throw new Exception(string.Format("Could not get registry key for service '{0}'.", serviceName));
        }
    }
}