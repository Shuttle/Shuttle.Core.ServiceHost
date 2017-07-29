﻿using System;
using System.IO;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Core.ServiceHost
{
    public class ServiceConfiguration : IServiceConfiguration
    {
        private string _description;
        private string _displayName;
        private string _serviceName;

        public ServiceConfiguration()
        {
            var entryAssembly = Assembly.GetEntryAssembly();

            if (entryAssembly != null)
            {
                _serviceName = entryAssembly.GetName().Name;
            }

            Instance = string.Empty;
            Username = string.Empty;
            Password = string.Empty;
            ServicePath = string.Empty;

            StartMode = ServiceStartMode.Automatic;
        }

        public string ServiceName
        {
            get
            {
                if (string.IsNullOrEmpty(_serviceName))
                {
                    throw new InvalidOperationException("No service name has been specified.");
                }

                return _serviceName;
            }
        }

        public string Instance { get; private set; }
        public string ServicePath { get; private set; }

        public IServiceConfiguration WithServiceName(string serviceName)
        {
            Guard.AgainstNullOrEmptyString(serviceName, nameof(serviceName));

            _serviceName = serviceName;

            return this;
        }

        public IServiceConfiguration WithInstance(string instance)
        {
            Guard.AgainstNullOrEmptyString(instance, nameof(instance));

            Instance = instance;

            return this;
        }

        public IServiceConfiguration WithServicePath(string path, int executionTimeout)
        {
            Guard.AgainstNullOrEmptyString(path, nameof(path));

            if (!File.Exists(path))
            {
                throw new InvalidOperationException($"Could not find remote assembly path '{path}'.");
            }

            ServicePath = path;
            ExecutionTimeout = executionTimeout < 30 ? 30 : executionTimeout;

            return this;
        }

        public int ExecutionTimeout { get; private set; }

        public string Username { get; private set; }
        public string Password { get; private set; }

        public string DisplayName => !string.IsNullOrEmpty(_displayName)
            ? _displayName
            : ServiceName;

        public string Description => !string.IsNullOrEmpty(_description)
            ? _description
            : $"Shuttle.Core.ServiceHost for {ServiceName}";

        public ServiceStartMode StartMode { get; private set; }

        public IServiceConfiguration WithUsername(string username)
        {
            Guard.AgainstNullOrEmptyString(username, nameof(username));

            Username = username;

            return this;
        }

        public IServiceConfiguration WithPassword(string password)
        {
            Guard.AgainstNullOrEmptyString(password, nameof(password));

            Password = password;

            return this;
        }

        public IServiceConfiguration WithDisplayName(string displayName)
        {
            Guard.AgainstNullOrEmptyString(displayName, nameof(displayName));

            _displayName = displayName;

            return this;
        }

        public IServiceConfiguration WithDescription(string description)
        {
            Guard.AgainstNullOrEmptyString(description, nameof(description));

            _description = description;

            return this;
        }

        public IServiceConfiguration WithServiceStartMode(ServiceStartMode startMode)
        {
            if (!Enum.IsDefined(typeof(ServiceStartMode), startMode))
            {
                throw new InvalidOperationException($"An invalid ServiceStartMode of '{startMode}' has been used to configure the service.");
            }

            StartMode = startMode;

            return this;
        }

        public IServiceConfiguration WithArguments(Arguments arguments)
        {
            Guard.AgainstNull(arguments, nameof(arguments));

            _serviceName = arguments.Get("ServiceName", _serviceName);
            Instance = arguments.Get("Instance", Instance);
            _displayName = arguments.Get("DisplayName", _displayName);
            _description = arguments.Get("Description", _description);
            Username = arguments.Get("Username", Username);
            Password = arguments.Get("Password", Password);

            if (arguments.Contains("StartMode"))
            {
                ServiceStartMode startMode;

                var value = arguments.Get<string>("StartMode");

                if (Enum.TryParse(value, true, out startMode))
                {
                    StartMode = startMode;
                }
                else
                {
                    throw new InvalidOperationException($"An invalid ServiceStartMode of '{value}' has been used to configure the service.");
                }
            }

            return this;
        }

        public string GetInstancedServiceName()
        {
            return string.Concat(ServiceName, string.IsNullOrEmpty(Instance)
                ? string.Empty
                : string.Format("${0}", Instance));
        }

        public string CommandLine()
        {
            var result = new StringBuilder();

            result.Append($"/serviceName=\"{ServiceName}\"");

            if (!string.IsNullOrEmpty(Instance))
            {
                result.Append($" /instance=\"{Instance}\"");
            }

            if (!string.IsNullOrEmpty(_displayName))
            {
                result.Append($" /displayName=\"{_displayName}\"");
            }

            if (!string.IsNullOrEmpty(_description))
            {
                result.Append($" /description=\"{_description}\"");
            }

            if (!string.IsNullOrEmpty(Username))
            {
                result.Append($" /username=\"{Username}\"");
            }

            if (!string.IsNullOrEmpty(Password))
            {
                result.Append($" /password=\"{Password}\"");
            }

            result.Append($" /startMode=\"{StartMode}\"");

            return result.ToString();
        }
    }
}