using Shuttle.Core.Infrastructure;

namespace Shuttle.Core.ServiceHost
{
    public static class ServiceConfiguratorExtensions
    {
        public static string InstancedServiceName(this IServiceConfigurator configurator)
        {
            Guard.AgainstNull(configurator, nameof(configurator));

            return string.Concat(configurator.ServiceName, string.IsNullOrEmpty(configurator.Instance)
                ? string.Empty
                : string.Format("${0}", configurator.Instance));
        }
    }
}