using System.Configuration;
using System.ServiceProcess;
using Shuttle.Core.Configuration;

namespace Shuttle.Core.ServiceHost
{
    public class ServiceHostSection : ConfigurationSection
    {
        [ConfigurationProperty("serviceName", IsRequired = true)]
        public string ServiceName => (string)this["serviceName"];

        [ConfigurationProperty("instance", IsRequired = false)]
        public string Instance => (string)this["instance"];

        [ConfigurationProperty("username", IsRequired = false)]
        public string Username => (string)this["username"];

        [ConfigurationProperty("password", IsRequired = false)]
        public string Password => (string)this["password"];

        [ConfigurationProperty("startMode", IsRequired = false, DefaultValue = ServiceStartMode.Automatic)]
        public ServiceStartMode StartMode => (ServiceStartMode)this["startMode"];

        public static IServiceConfiguration Configuration()
        {
            var section = ConfigurationSectionProvider.Open<ServiceHostSection>("shuttle", "service");
            var configuration = new ServiceConfiguration();

            if (section != null)
            {
                configuration.WithServiceName(section.ServiceName);

                if (!string.IsNullOrEmpty(section.Instance))
                {
                    configuration.WithInstance(section.Instance);
                }

                if (!string.IsNullOrEmpty(section.Username))
                {
                    configuration.WithUsername(section.Username);
                }

                if (!string.IsNullOrEmpty(section.Password))
                {
                    configuration.WithPassword(section.Password);
                }

                configuration.WithStartMode(section.StartMode);
            }

            return configuration;
        }
    }
}