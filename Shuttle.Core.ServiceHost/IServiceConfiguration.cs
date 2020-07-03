using System.ServiceProcess;
using Shuttle.Core.Cli;

namespace Shuttle.Core.ServiceHost
{
    public interface IServiceConfiguration
    {
        string ServiceName { get; }
        string Instance { get; }
        string ServicePath { get; }
        string Username { get; }
        string Password { get; }
        string DisplayName { get; }
        string Description { get; }
        int Timeout { get; }
        ServiceStartMode StartMode { get; }
        bool DelayedAutoStart { get; }
        IServiceConfiguration WithServiceName(string serviceName);
        IServiceConfiguration WithInstance(string instance);
        IServiceConfiguration WithServicePath(string path);
        IServiceConfiguration WithUsername(string username);
        IServiceConfiguration WithPassword(string password);
        IServiceConfiguration WithDisplayName(string displayName);
        IServiceConfiguration WithDescription(string description);
        IServiceConfiguration WithStartMode(ServiceStartMode startMode);
        IServiceConfiguration WithArguments(Arguments arguments);
        IServiceConfiguration WithTimeout(int timeout);
        IServiceConfiguration WithDelayedAutoStart();
        string GetInstancedServiceName();
        string CommandLine();
    }
}