using System;
using System.Threading;

namespace Shuttle.Core.ServiceHost
{
    public static class ConsoleExtensions
    {
        private static readonly object Lock = new object();

        public static void WriteLine(ConsoleColor color, string message)
        {
            lock (Lock)
            {
                var foregroundColor = Console.ForegroundColor;

                Console.ForegroundColor = color;
                Console.WriteLine(message);
                Console.ForegroundColor = foregroundColor;
            }
        }
    }
}