using System.ServiceProcess;
using Shuttle.Core.Infrastructure;

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
        int ExecutionTimeout { get; }
        ServiceStartMode StartMode { get; }
        IServiceConfiguration WithServiceName(string serviceName);
        IServiceConfiguration WithInstance(string instance);
        IServiceConfiguration WithServicePath(string path, int executionTimeout);
        IServiceConfiguration WithUsername(string username);
        IServiceConfiguration WithPassword(string password);
        IServiceConfiguration WithDisplayName(string displayName);
        IServiceConfiguration WithDescription(string description);
        IServiceConfiguration WithServiceStartMode(ServiceStartMode startMode);
        IServiceConfiguration WithArguments(Arguments arguments);
        string GetInstancedServiceName();
        string CommandLine();
    }
}