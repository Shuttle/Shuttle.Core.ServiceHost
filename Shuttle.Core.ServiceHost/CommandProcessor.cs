using System;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Core.ServiceHost
{
    public class CommandProcessor
    {
        public bool Execute()
        {
            var arguments = new Arguments(Environment.GetCommandLineArgs());

            return true;
        }
    }
}