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
            [Frozen] IServiceCollection services,
            [Frozen] ICosmosContainerRegistry registry,
            CosmosContainerBuilder sut)
        {
            sut.AddResource<Record>();

            services
                .Received(1)
                .Add(Arg.Is<ServiceDescriptor>(s
                    => s.ServiceType
                    == typeof(ICosmosContainerNameProvider)));

            registry
                .Received(1)
                .Register<Record>(sut.ContainerName, sut.DatabaseName);
        }

        // Test double registration will fail
        // Test same container in different databases will fail
    }
}