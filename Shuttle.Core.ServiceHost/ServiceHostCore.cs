using System;
using Shuttle.Core.Contract;
using Shuttle.Core.Logging;

namespace Shuttle.Core.ServiceHost
{
    public sealed class ServiceHost
    {
        private readonly ILog _log;

        private ServiceHost()
        {
            _log = Log.For(this);

            AppDomain.CurrentDomain.UnhandledException += UnhandledException;
        }

        private void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = (Exception) e.ExceptionObject;

            _log.Error(ex.Message);
        }

        public static void Run<T>() where T : IServiceHostStart, new()
        {
            Run(new T());
        }

        public static void Run(IServiceHostStart service)
        {
            Guard.AgainstNull(service, nameof(service));

            try
            {
                Console.CursorVisible = false;

                new ConsoleService(service).Execute();
            }
            catch (Exception ex)
            {
                Log.For(typeof(ServiceHost)).Fatal(ex.Message);
                throw;
            }
        }
    }
}