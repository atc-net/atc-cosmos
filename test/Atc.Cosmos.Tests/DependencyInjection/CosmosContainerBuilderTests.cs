using System.Linq;
using Atc.Cosmos.DependencyInjection;
using Atc.Cosmos.Internal;
using Atc.Test;
using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace Atc.Cosmos.Tests.DependencyInjection
{
    public class CosmosContainerBuilderTests
    {
        [Theory, AutoNSubstituteData]
        public void AddResource_Registers_ICosmosConntainerNameProvider(
            [Frozen] string containerName,
            [Frozen] IServiceCollection services,
            CosmosContainerBuilder sut)
        {
            sut.AddResource<Record>();

            services
                .Received(1)
                .Add(Arg.Is<ServiceDescriptor>(s
                    => s.ServiceType
                    == typeof(ICosmosContainerNameProvider)));

            var nameProvider = services
                .ReceivedCalls()
                .SelectMany(c => c.GetArguments())
                .OfType<ServiceDescriptor>()
                .Where(s
                    => s.ServiceType
                    == typeof(ICosmosContainerNameProvider))
                .Select(s => s.ImplementationInstance)
                .OfType<ICosmosContainerNameProvider>()
                .Single();

            nameProvider
                .GetContainerName(typeof(Record))
                .Should()
                .Be(containerName);
        }
    }
}