# Shuttle.Core.ServiceHost

Turns your console application into a Windows service.

A typical implementation would be as follows:

~~~ c#
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
~~~

Implement the `IServiceHost` interface if you need both `Start()` and `Stop()` methods; else `IServiceHostStart` for `Start()` and `IServiceHostStop` for `Stop()` although there would be little value in having only a `Stop()`.  If you do not need a `Stop()` method or you prefer using `IDisposable` to handle the destruction then you would go with only the `IServiceHostStart` interface.

## Running the Service

The following methods are available to get this going on the `ServiceHost` class:

~~~ c#
public static void Run<T>() where T : IServiceHostStart, new()
public static void Run<T>(Action<IServiceConfiguration> configure) where T : IServiceHostStart, new()
public static void Run(IServiceHostStart service)
public static void Run(IServiceHostStart service, Action<IServiceConfiguration> configure)
~~~

The `IServiceConfiguration` allows you to configure the service from code.  Configuration supplied through code has the highest precedence.

## Configuration Section

You may also specify configuration using the following configuration which may, as all Shuttle configuration sections do, be grouped under a `shuttle` group.

~~~ xml
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
~~~

## Command Line

The following command-line arguments are available and can be viewed by running `{your-console}.exe /?`:

~~~
[/install [/serviceName]]	
	- install the service
		
[/displayName]				
	- friendly name for the installed service
		
[/description]				
	- Description for the service
		
[/instance]					
	- unique name of the instance you wish to install
		
[/startMode]			
	- specifies that the service start mode (Boot, System, Automatic, Manual, Disabled)
		
[/username /password]
	- username and password of the account to use for the service
- or -
	
[/uninstall [/serviceName] [/instance]]	

[/start]
	- starts the service instance

[/stop]
	- stops the service instance
~~~

## Service Name

If no `/serviceName` is specified the full name of the your console application along with the version number, e.g.:

~~~
Namespace.ConsoleApplication (1.0.0.0)
~~~

## Uninstall

If you set the `/serviceName` and/or `/instance` during installation you will need to specify them when uninstalling as well, e.g.:

~~~
{your=console}.exe 
	/uninstall 
	/serviceName:"Shuttle.Application.Server" 
	/instance:"Instance5"
~~~

## Example

~~~
{your=console}.exe 
	/install 
	/serviceName:"Shuttle.Application.Server" 
	/displayName:"Shuttle server for the application"
	/description:"Service to handle messages relating to the application." 
	/username:"domain\hostuser"
	/password:"p@ssw0rd!"
~~~

