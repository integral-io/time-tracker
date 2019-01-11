using System;
using Xunit;
using TimeTracker.Api;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Controllers;
using System.Threading.Tasks;
using FluentAssertions;

namespace TimeTracker.Api.Test
{
    public class HelloWorldControllerTests
    {
        protected HelloWorldController GetSut() {
            IServiceCollection service = new ServiceCollection();

            service.AddTransient<HelloWorldController>();

            var serviceProvider = service.BuildServiceProvider();

            return serviceProvider.GetService<HelloWorldController>();
        }

        [Fact]
        public async Task CanSayHello()
        {
            var sut = GetSut();

            var result = await sut.SayHello();

            result.Value.Should().Be("Hello from the backend");
        }
    }
}
