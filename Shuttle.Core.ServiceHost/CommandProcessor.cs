﻿using System;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Shuttle.Core.Cli;
using Shuttle.Core.Contract;
using Shuttle.Core.Logging;
using Shuttle.Core.Reflection;

namespace Shuttle.Core.ServiceHost
{
    public class CommandProcessor
    {
        public bool Execute(ServiceConfiguration configuration)
        {
            Guard.AgainstNull(configuration, nameof(configuration));

            var result = false;

            try
            {
                var arguments = new Arguments(Environment.GetCommandLineArgs());
                var action = arguments.CommandLine.Length > 1 ? arguments.CommandLine[1] : string.Empty;

                configuration.WithArguments(arguments);

                if (ShouldShowHelp(arguments))
                {
                    ShowHelp();

                    return true;
                }

                if (arguments.Contains("debug"))
                {
                    Debugger.Launch();
                }

                var install = arguments.Contains("install") || action.Equals("install", StringComparison.InvariantCultureIgnoreCase);
                var uninstall = arguments.Contains("uninstall") || action.Equals("uninstall", StringComparison.InvariantCultureIgnoreCase);

                if (install && uninstall)
                {
                    throw new InstallException("Cannot specify /install and /uninstall together.");
                }

                var start = arguments.Contains("start") || action.Equals("start", StringComparison.InvariantCultureIgnoreCase); 
                var stop = arguments.Contains("stop") || action.Equals("stop", StringComparison.InvariantCultureIgnoreCase); 
                var timeoutValue = arguments.Get("timeout", "30000");

                if (!int.TryParse(timeoutValue, out var timeout))
                {
                    timeout = 30000;
                }

                configuration.WithTimeout(timeout);

                if (uninstall)
                {
                    new WindowsServiceInstaller().Uninstall(configuration);

                    result= true;
                }

                if (install)
                {
                    new WindowsServiceInstaller().Install(configuration);

                    result = true;
                }

                if (stop || start)
                {
                    var controller = new ServiceHostController(configuration);

                    if (stop)
                    {
                        controller.Stop();
                    }

                    if (start)
                    {
                        controller.Start();
                    }

                    result = true;
                }
            }
            catch (Exception ex)
            {
                if (Environment.UserInteractive)
                {
                    ConsoleExtensions.WriteLine(ConsoleColor.Red, ex.AllMessages());

                    Console.WriteLine();
                    ConsoleExtensions.WriteLine(ConsoleColor.Gray, "Press any key to close...");
                    Console.ReadKey();
                }
                else
                {
                    Log.Fatal($"[UNHANDLED EXCEPTION] : exception = {ex.AllMessages()}");

                    throw;
                }

                result = true;
            }

            return result;
        }

        public bool ShouldShowHelp(Arguments arguments)
        {
            Guard.AgainstNull(arguments, "arguments");

            return arguments.Get("help", false) || arguments.Get("h", false) || arguments.Get("?", false);
        }

        protected static void ShowHelp()
        {
            try
            {
                using (
                    var stream =
                        Assembly.GetCallingAssembly().GetManifestResourceStream("Shuttle.Core.ServiceHost.Content.Help.txt"))
                {
                    if (stream == null)
                    {
                        Console.WriteLine("Error retrieving help content stream.");

                        return;
                    }

                    Console.WriteLine(new StreamReader(stream).ReadToEnd());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}