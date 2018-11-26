# Shuttle.Core.ServiceHost

```
PM> Install-Package Shuttle.Core.ServiceHost
```

Allows you to host your console application.  When using .Net 4.6+ your console may also be hosted as a Windows Service.  When using .Net Core 2.0+ your console hosting can be stopped by sending `ctrl+c` to the console.

A typical implementation would be as follows:

``` c#
using System;
using System.Threading;
using Shuttle.Core.Infrastructure;

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
```

Implement the `IServiceHost` interface if you need both `Start()` and `Stop()` methods; else `IServiceHostStart` for `Start()` and `IServiceHostStop` for `Stop()` although there would be little value in having only a `Stop()`.  If you do not need a `Stop()` method or you prefer using `IDisposable` to handle the destruction then you would go with only the `IServiceHostStart` interface.

## Running the host

The following methods are available to get this going on the `ServiceHost` class:

``` c#
public static void Run<T>() where T : IServiceHostStart, new()
public static void Run(IServiceHostStart service)
```

For .Net 4.6+ the following are also available:

``` c#
public static void Run<T>(Action<IServiceConfiguration> configure) where T : IServiceHostStart, new()
public static void Run(IServiceHostStart service, Action<IServiceConfiguration> configure)
```

The `IServiceConfiguration` allows you to configure the service from code.  Configuration supplied through code has the highest precedence.

## .Net 4.6+ options

### Configuration Section

You may also specify configuration using the following configuration which may, as all Shuttle configuration sections do, be grouped under a `shuttle` group.

``` xml
<configuration>
  <configSections>
    <section name="service" type="Shuttle.Core.ServiceHost.ServiceHostSection, Shuttle.Core.ServiceHost" />
  </configSections>

  <service
    serviceName="test-service"
    instance="one"
    username="mr.resistor"
    password="ohm"
    startMode="Disabled" />
</configuration>
```

### Command Line (.Net 4.6+ only)

The following command-line arguments are available and can be viewed by running `{your-console}.exe /?`:

```
{your-console}.exe [[/]action]

	[action]:
	- install (installs the service)
	- uninstall (uninstalls the service)
	- start (starts the service)
	- stop (stops the service)

[/serviceName="the-service-name"]
	- install the service
		
[/displayName="display-name"]				
	- friendly name for the installed service
		
[/description="description"]				
	- Description for the service
		
[/instance="instance-name"]
	- unique name of the instance you wish to install
		
[/startMode="start-mode"]
	- specifies that the service start mode (Boot, System, Automatic, Manual, Disabled)
		
[/username="username" /password="password"]
	- username and password of the account to use for the service
```

### Service Name

If no `/serviceName` is specified the full name of the your console application along with the version number, e.g.:

```
Namespace.ConsoleApplication (1.0.0.0)
```

### Action

If you set the `/serviceName` and/or `/instance` during installation you will need to specify them when using the other actions also as well, e.g.:

```
{your=console}.exe 
	uninstall|start|stop
	/serviceName:"Shuttle.Application.Server" 
	/instance:"Instance5"
```

### Example

```
{your=console}.exe 
	install 
	/serviceName:"Shuttle.Application.Server" 
	/displayName:"Shuttle server for the application"
	/description:"Service to handle messages relating to the application." 
	/username:"domain\hostuser"
	/password:"p@ssw0rd!"
```

