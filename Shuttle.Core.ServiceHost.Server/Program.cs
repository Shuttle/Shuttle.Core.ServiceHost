using System;
using System.Threading;
using Shuttle.Core.Threading;

namespace Shuttle.Core.ServiceHost.Server
{
    internal class Program
    {
        private static void Main()
        {
            ServiceHost.Run<TestHost>();
        }

        public class TestHost : IServiceHost
        {
            private readonly Thread _thread;
            private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

            public TestHost()
            {
                _thread = new Thread(Worker);
            }

            public void Start()
            {
                _thread.Start();
            }

            public void Stop()
            {
                _cancellationTokenSource.Cancel();
                _thread.Join(5000);
            }

            private void Worker()
            {
                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    Console.WriteLine($"[working] : {DateTime.Now:O}");
                    ThreadSleep.While(1000, _cancellationTokenSource.Token);
                }
            }
        }
    }
}