using System.Linq;
using AutoFixture.Xunit2;
using Atc.Cosmos.Extensions;
using Atc.Cosmos.Internal;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Atc.Cosmos.Tests.Extensions
{
    public class CosmosContainerBuilderTests
    {
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

            nameProvider.ContainerName
                .Should()
                .Be(containerName);
            nameProvider.FromType
                .Should()
                .Be(typeof(Record));
        }
    }
}