﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Core.ServiceHost
{
    public class Host
    {
        private bool _runServiceException;

        public void RunService(HostServiceConfiguration hostServiceConfiguration)
        {
            Guard.AgainstNull(hostServiceConfiguration, "hostServiceConfiguration");

            var log = new HostEventLog(hostServiceConfiguration.ServiceName);

            try
            {
                var configurationFile = GetHostConfigurationFile(hostServiceConfiguration);

                string message;

                if (!File.Exists(configurationFile))
                {
                    message = string.Format("Cannot find host configuration file '{0}'", configurationFile);

                    log.WrinteEntry(message, EventLogEntryType.Error);

                    throw new ApplicationException(message);
                }

                message = string.Format("[configuring] : service name = '{0}' / configuration file '{1}'",
                    hostServiceConfiguration.ServiceName, configurationFile);

                if (!Environment.UserInteractive)
                {
                    log.WrinteEntry(message);
                }
                else
                {
                    ColoredConsole.WriteLine(ConsoleColor.Cyan, message);
                }

                AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", configurationFile);

                var state = typeof(ConfigurationManager).GetField("s_initState",
                    BindingFlags.NonPublic | BindingFlags.Static);

                if (state == null)
                {
                    throw new ApplicationException("Could not obtain \'s_initState\' from ConfigurationManager.");
                }

                state.SetValue(null, 0);

                var system = typeof(ConfigurationManager).GetField("s_configSystem",
                    BindingFlags.NonPublic | BindingFlags.Static);

                if (system == null)
                {
                    throw new ApplicationException("Could not obtain \'s_configSystem\' from ConfigurationManager.");
                }

                system.SetValue(null, null);

                var current = typeof(ConfigurationManager)
                    .Assembly.GetTypes().First(x => x.FullName == "System.Configuration.ClientConfigPaths")
                    .GetField("s_current", BindingFlags.NonPublic | BindingFlags.Static);

                if (current == null)
                {
                    throw new ApplicationException(
                        "Could not obtain 's_current' from System.Configuration.ClientConfigPaths.");
                }

                current.SetValue(null, null);

            }
            catch (Exception ex)
            {
                if (Environment.UserInteractive)
                {
                    _runServiceException = true;

                    ColoredConsole.WriteLine(ConsoleColor.Red, ex.AllMessages());
                    ColoredConsole.WriteLine(ConsoleColor.Red, ex.StackTrace);

                    Console.WriteLine();
                    ColoredConsole.WriteLine(ConsoleColor.DarkYellow, "[press any key to close]");
                    Console.ReadKey();
                }
                else
                {
                    log.WrinteEntry(ex.Message, EventLogEntryType.Error);

                    throw;
                }
            }
        }

        private void ConsoleService(HostServiceConfiguration hostServiceConfiguration)
        {
            Guard.AgainstNull(hostServiceConfiguration, "hostServiceConfiguration");

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

            Console.CancelKeyPress += ((sender, e) =>
            {
                if (!_runServiceException)
                {
                    ColoredConsole.WriteLine(ConsoleColor.Green, "[stopping]");
                }
                else
                {
                    ColoredConsole.WriteLine(ConsoleColor.DarkYellow,
                        "[press any key to close (ctrl+c does not work)]");
                }

                waitHandle.Set();

                e.Cancel = true;
            });

            hostServiceConfiguration.Host.Start();

            Console.WriteLine();
            ColoredConsole.WriteLine(ConsoleColor.Green, "Shuttle.Core.Host started for '{0}'.",
                hostServiceConfiguration.ServiceName);
            Console.WriteLine();
            ColoredConsole.WriteLine(ConsoleColor.DarkYellow, "[press ctrl+c to stop]");
            Console.WriteLine();

            WaitHandle.WaitAny(waitHandles);

            var disposable = hostServiceConfiguration.Host as IDisposable;

            if (disposable != null)
            {
                disposable.Dispose();
            }
        }

        private static string GetHostConfigurationFile(HostServiceConfiguration hostServiceConfiguration)
        {
            return Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                string.IsNullOrEmpty(hostServiceConfiguration.ConfigurationFileName)
                    ? string.Concat(hostServiceConfiguration.Host.GetType().Assembly.ManifestModule.Name, ".config")
                    : hostServiceConfiguration.ConfigurationFileName);
        }
    }
}