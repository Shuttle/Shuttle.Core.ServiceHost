using System;
using System.ServiceProcess;
using NUnit.Framework;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Core.ServiceHost.Tests
{
    [TestFixture]
    public class ServiceConfigurationFixture
    {
        [Test]
        public void Should_be_able_to_get_command_line()
        {
            var commandLine = new ServiceConfiguration()
                .WithServiceName("Server")
                .WithInstance("One")
                .WithDisplayName("Server Display Name")
                .WithDescription("Service Description")
                .WithUsername("mr.resistor")
                .WithPassword("ohm")
                .WithStartMode(ServiceStartMode.Disabled)
                .CommandLine();

            Assert.AreEqual(
                "/serviceName=\"Server\" /instance=\"One\" /displayName=\"Server Display Name\" /description=\"Service Description\" /username=\"mr.resistor\" /password=\"ohm\" /startMode=\"Disabled\"",
                commandLine);
        }

        [Test]
        public void Should_be_able_to_get_configuration_from_empty_arguments()
        {
            var configuration = new ServiceConfiguration();

            Assert.Throws<InvalidOperationException>(() => Console.WriteLine(configuration.ServiceName));
            Assert.AreEqual(string.Empty, configuration.Instance);
            Assert.Throws<InvalidOperationException>(() => Console.WriteLine(configuration.GetInstancedServiceName()));
            Assert.Throws<InvalidOperationException>(() => Console.WriteLine(configuration.DisplayName));
            Assert.Throws<InvalidOperationException>(() => Console.WriteLine(configuration.Description));
            Assert.AreEqual(string.Empty, configuration.Username);
            Assert.AreEqual(string.Empty, configuration.Password);
            Assert.AreEqual(ServiceStartMode.Automatic, configuration.StartMode);
            Assert.AreEqual(string.Empty, configuration.ServicePath);
        }

        [Test]
        public void Should_be_able_to_get_configuration_from_full_arguments()
        {
            var arguments = new Arguments(
                "/serviceName=Server",
                "/displayName:\"Server Display Name\"",
                "/description:\"Server Description\"",
                "/instance=One",
                "/username=mr.resistor",
                "/password=ohm",
                "/startMode=Manual");

            var configuration = new ServiceConfiguration().WithArguments(arguments);

            Assert.AreEqual("Server", configuration.ServiceName);
            Assert.AreEqual("One", configuration.Instance);
            Assert.AreEqual("Server$One", configuration.GetInstancedServiceName());
            Assert.AreEqual("Server Display Name", configuration.DisplayName);
            Assert.AreEqual("Server Description", configuration.Description);
            Assert.AreEqual("mr.resistor", configuration.Username);
            Assert.AreEqual("ohm", configuration.Password);
            Assert.AreEqual(ServiceStartMode.Manual, configuration.StartMode);
            Assert.AreEqual(string.Empty, configuration.ServicePath);
        }

        [Test]
        public void Should_throw_when_an_invalid_start_mode_has_been_configured()
        {
            var arguments = new Arguments("/startMode=InvalidStartMode");

            var ex = Assert.Throws<InvalidOperationException>(() => new ServiceConfiguration().WithArguments(arguments));

            Assert.IsTrue(ex.Message.Contains("An invalid ServiceStartMode"));
            Assert.IsTrue(ex.Message.Contains("InvalidStartMode"));
        }
    }
}