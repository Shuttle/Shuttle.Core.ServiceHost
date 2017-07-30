﻿using System;
using System.Collections;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Security;
using System.Security.Principal;
using Microsoft.Win32;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Core.ServiceHost
{
    public class WindowsServiceInstaller : MarshalByRefObject
    {
        public static readonly string ServiceConfigurationKey = "__ServiceConfigurationKey__";

        public void Install(IServiceConfiguration configuration)
        {
            Guard.AgainstNull(configuration, nameof(configuration));

            GuardAdministrator();

            if (!string.IsNullOrEmpty(configuration.ServicePath))
            {
                new ServiceInvoker(configuration).Execute(ServiceCommand.Install);

                return;
            }

            CallContext.LogicalSetData(ServiceConfigurationKey, configuration);

            var instancedServiceName = configuration.GetInstancedServiceName();
            var log = ServiceHostEventLog.GetEventLog(instancedServiceName);

            ColoredConsole.WriteLine(ConsoleColor.Green, "Installing service '{0}'.", instancedServiceName);

            var entryAssembly = Assembly.GetEntryAssembly();

            if (entryAssembly == null)
            {
                throw new InvalidOperationException("An entry assembly is required in order to install a service.");
            }

            var entryAssemblyLocation = entryAssembly.Location;

            if (string.IsNullOrEmpty(entryAssemblyLocation))
            {
                throw new InvalidOperationException("The entry assembly has no location.");
            }

            if (!Path.GetExtension(entryAssemblyLocation).Equals(".exe", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new InvalidOperationException("The entry assembly must be an 'exe' in order to install as a service.");
            }

            var assemblyInstaller = new AssemblyInstaller(typeof(ServiceHost).Assembly, null);

            using (var installer = assemblyInstaller)
            {
                IDictionary state = new Hashtable();

                installer.UseNewContext = true;

                try
                {
                    installer.Install(state);
                    installer.Commit(state);

                    var serviceKey = GetServiceKey(instancedServiceName);

                    serviceKey.SetValue("Description", configuration.Description);
                    serviceKey.SetValue("ImagePath",
                        string.Format("{0} /serviceName=\"{1}\"{2}",
                            entryAssemblyLocation,
                            configuration.ServiceName,
                            string.IsNullOrEmpty(configuration.Instance)
                                ? string.Empty
                                : string.Format(" /instance=\"{0}\"", configuration.Instance)));
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

                var message = string.Format("Service '{0}' has been successfully installed.", instancedServiceName);

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

        public void Uninstall(IServiceConfiguration configuration)
        {
            GuardAdministrator();

            Guard.AgainstNull(configuration, "configuration");

            if (!string.IsNullOrEmpty(configuration.ServicePath))
            {
                new ServiceInvoker(configuration).Execute(ServiceCommand.Uninstall);

                return;
            }

            CallContext.LogicalSetData(ServiceConfigurationKey, configuration);

            var instancedServiceName = configuration.GetInstancedServiceName();
            var log = ServiceHostEventLog.GetEventLog(instancedServiceName);

            ColoredConsole.WriteLine(ConsoleColor.Green, "Uninstalling service '{0}'.", instancedServiceName);

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

            var message = string.Format("Service '{0}' has been successfully uninstalled.", instancedServiceName);

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