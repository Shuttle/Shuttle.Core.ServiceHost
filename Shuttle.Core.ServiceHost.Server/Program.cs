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

        public class TestHost : IServiceHost, IThreadState
        {
            private readonly Thread _thread;
            private volatile bool _active;

            public TestHost()
            {
                _thread = new Thread(Worker);
            }

            public void Start()
            {
                _active = true;
                _thread.Start();
            }

            public void Stop()
            {
                _active = false;
                _thread.Join(5000);
            }

            public bool Active => _active;

            private void Worker()
            {
                while (_active)
                {
                    Console.WriteLine($"[working] : {DateTime.Now:O}");
                    ThreadSleep.While(1000, this);
                }
            }
        }
    }
}