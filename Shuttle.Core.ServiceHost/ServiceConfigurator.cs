using System;
using System.IO;
using System.Reflection;
using System.ServiceProcess;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Core.ServiceHost
{
    public class ServiceConfigurator : IServiceConfigurator
    {
        public string ServiceName { get; private set; }
        public string Instance { get; private set; }
        public string ServiceAssemblyPath { get; private set; }

        public IServiceConfigurator WithServiceName(string name)
        {
            Guard.AgainstNullOrEmptyString(name, nameof(name));

            ServiceName = name;

            return this;
        }

        public IServiceConfigurator WithInstance(string name)
        {
            Guard.AgainstNullOrEmptyString(name, nameof(name));

            Instance = name;

            return this;
        }

        public IServiceConfigurator WithServiceAssemblyPath(string path)
        {
            Guard.AgainstNullOrEmptyString(path, nameof(path));

            if (!File.Exists(path))
            {
                throw new InvalidOperationException($"Could not find remote assembly path '{path}'.");
            }

            ServiceAssemblyPath = path;

            return this;
        }

        public IServiceConfigurator WithArguments(Arguments arguments)
        {
            Guard.AgainstNull(arguments, nameof(arguments));

            if (string.IsNullOrEmpty(ServiceName))
            {
                ServiceName = arguments.Get("serviceName", Assembly.GetEntryAssembly().FullName);
            }

            return this;
        }

        public string Username { get; private set; }
        public string Password { get; private set; }
        public string DisplayName { get; private set; }
        public string Description { get; private set; }
        public ServiceStartMode ServiceStartMode { get; private set; }

        public IServiceConfigurator WithUsername(string username)
        {
            Guard.AgainstNullOrEmptyString(username, nameof(username));

            Username = username;

            return this;
        }

        public IServiceConfigurator WithPassword(string password)
        {
            Guard.AgainstNullOrEmptyString(password, nameof(password));

            Password = password;

            return this;
        }

        public IServiceConfigurator WithDisplayName(string name)
        {
            Guard.AgainstNullOrEmptyString(name, nameof(name));

            DisplayName = name;

            return this;
        }

        public IServiceConfigurator WithDescription(string description)
        {
            Guard.AgainstNullOrEmptyString(description, nameof(description));

            Description = description;

            return this;
        }

        public IServiceConfigurator WithServiceStartMode(ServiceStartMode startMode)
        {
            if (!Enum.IsDefined(typeof(ServiceStartMode), startMode))
            {
                throw new InvalidOperationException(
                    string.Format(
                        "An invalid ServiceStartMode of '{0}' has been used to configure the service.", startMode));
            }

            ServiceStartMode = startMode;

            return this;
        }
    }
}