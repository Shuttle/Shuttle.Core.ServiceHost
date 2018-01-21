using System;
using System.IO;
using System.ServiceProcess;
using NUnit.Framework;
using Shuttle.Core.Configuration;

namespace Shuttle.Core.ServiceHost.Tests
{
    [TestFixture]
    public class ServiceHostSectionFixture
    {
        [Test]
        [TestCase("ServiceHost.config")]
        [TestCase("ServiceHost-Grouped.config")]
        public void Should_be_able_to_load_the_configuration(string file)
        {
            var section = ConfigurationSectionProvider.OpenFile<ServiceHostSection>("shuttle", "service",
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"config-files\{file}"));

            Assert.IsNotNull(section);
            Assert.AreEqual("test-service", section.ServiceName);
            Assert.AreEqual("one", section.Instance);
            Assert.AreEqual("mr.resistor", section.Username);
            Assert.AreEqual("ohm", section.Password);
            Assert.AreEqual(ServiceStartMode.Disabled, section.StartMode);
        }
    }
}