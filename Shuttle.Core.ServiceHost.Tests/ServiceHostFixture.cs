using System;
using System.IO;
using NUnit.Framework;

namespace Shuttle.Core.ServiceHost.Tests
{
    [TestFixture]
    [Explicit]
    public class ServiceHostFixture
    {
        private string GetServicePath()
        {
            var servicePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Shuttle.Core.ServiceHost.Server\bin\Debug\Shuttle.Core.ServiceHost.Server.exe");

            if (!File.Exists(servicePath))
            {
                throw new ApplicationException($"Service '{servicePath}' does not exist.  The project may not have been built.");
            }

            return servicePath;
        }

        [Test]
        public void Should_be_able_to_install()
        {
            new WindowsServiceInstaller().Install(new ServiceConfiguration()
                .WithServiceName("Shuttle.Core.ServiceHost.Server")
                .WithServicePath(GetServicePath(), 30));
        }

        [Test]
        public void Should_be_able_to_uninstall()
        {
            new WindowsServiceInstaller().Uninstall(new ServiceConfiguration()
                .WithServiceName("Shuttle.Core.ServiceHost.Server")
                .WithServicePath(GetServicePath(), 30));
        }
    }
}