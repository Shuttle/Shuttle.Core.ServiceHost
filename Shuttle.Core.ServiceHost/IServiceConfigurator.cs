using System.ServiceProcess;

namespace Shuttle.Core.ServiceHost
{
    public interface IServiceConfigurator
    {
        string ServiceName { get; }
        string Instance { get; }
        string ServiceAssemblyPath { get; }
        string Username { get; }
        string Password { get; }
        string DisplayName { get; }
        string Description { get; }
        ServiceStartMode ServiceStartMode { get; }

        IServiceConfigurator WithServiceName(string name);
        IServiceConfigurator WithInstance(string name);
        IServiceConfigurator WithServiceAssemblyPath(string path);
        IServiceConfigurator WithUsername(string username);
        IServiceConfigurator WithPassword(string password);
        IServiceConfigurator WithDisplayName(string name);
        IServiceConfigurator WithDescription(string description);
        IServiceConfigurator WithServiceStartMode(ServiceStartMode startMode);
    }
}