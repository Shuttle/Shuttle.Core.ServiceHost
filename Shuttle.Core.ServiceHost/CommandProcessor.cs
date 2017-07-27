using System;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Core.ServiceHost
{
    public class CommandProcessor
    {
        public bool Execute(ServiceConfigurator configurator)
        {
            Guard.AgainstNull(configurator, nameof(configurator));

            try
            {
                var arguments = new Arguments(Environment.GetCommandLineArgs());

                configurator.WithArguments(arguments);

                if (ShouldShowHelp(arguments))
                {
                    ShowHelp();

                    return true;
                }

                if (arguments.Contains("debug"))
                {
                    Debugger.Launch();
                }

                var install = arguments.Get("install", string.Empty);
                var uninstall = arguments.Get("uninstall", string.Empty);

                if (!string.IsNullOrEmpty(install) && !string.IsNullOrEmpty(uninstall))
                {
                    throw new InstallException("Cannot specify /install and /uninstall together.");
                }

                if (!string.IsNullOrEmpty(uninstall))
                {
                    new WindowsServiceInstaller().Uninstall(configurator);

                    return true;
                }

                if (!string.IsNullOrEmpty(install))
                {
                    new WindowsServiceInstaller().Install(configurator);

                    return true;
                }
            }
            catch (Exception ex)
            {
                if (Environment.UserInteractive)
                {
                    ColoredConsole.WriteLine(ConsoleColor.Red, ex.AllMessages());

                    Console.WriteLine();
                    ColoredConsole.WriteLine(ConsoleColor.Gray, "Press any key to close...");
                    Console.ReadKey();
                }
                else
                {
                    Log.Fatal(string.Format("[UNHANDLED EXCEPTION] : exception = {0}", ex.AllMessages()));

                    throw;
                }
            }
            return false;
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