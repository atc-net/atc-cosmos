using System;
using System.Linq;
using Atc.Cosmos.Internal;
using Atc.Test;
using AutoFixture.AutoNSubstitute;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Xunit;

namespace Atc.Cosmos.Tests.Internal
{
    public class CosmosContainerProviderTests
    {
        [Theory, AutoNSubstituteData]
        public void GetContainer_Return_Specified_Container(
            ICosmosClientProvider clientProvider,
            [Substitute] CosmosClient cosmosClient,
            OptionsWrapper<CosmosOptions> options,
            [Substitute] Container container,
            [Substitute] ICosmosContainerRegistry containerRegistry,
            string containerName)
        {
            containerRegistry
                .DefaultOptions
                .Returns(options.Value);
            clientProvider
                .GetClient(options.Value)
                .Returns(cosmosClient);
            cosmosClient
                .GetContainer(default, default)
                .ReturnsForAnyArgs(container);

            var sut = new CosmosContainerProvider(
                clientProvider,
                containerRegistry);

            sut.GetContainer(containerName)
                .Should()
                .Be(container);

            clientProvider
                .Received(1)
                .GetClient(options.Value);

            cosmosClient
                .Received(1)
                .GetContainer(
                    options.Value.DatabaseName,
                    containerName);
        }

        [Theory, AutoNSubstituteData]
        public void GetContainer_Return_NamedContainer(
            ICosmosClientProvider clientProvider,
            [Substitute] CosmosClient cosmosClient,
            OptionsWrapper<CosmosOptions> options,
            ICosmosContainerNameProvider provider,
            [Substitute] Container container,
            [Substitute] ICosmosContainerRegistry containerRegistry,
            string providerName)
        {
            clientProvider
                .GetClient(options.Value)
                .Returns(cosmosClient);
            cosmosClient
                .GetContainer(default, default)
                .ReturnsForAnyArgs(container);
            provider
                .IsForType(typeof(string))
                .Returns(true);
            provider
                .ContainerName
                .Returns(providerName);
            provider
                .Options
                .Returns(options.Value);
            containerRegistry
                .DefaultOptions
                .Returns(options.Value);
            containerRegistry
                .GetContainerForType<string>()
                .Returns(provider);

            var sut = new CosmosContainerProvider(
                clientProvider,
                containerRegistry);

            sut.GetContainer<string>()
                .Should()
                .Be(container);

            containerRegistry
                .Received(1)
                .GetContainerForType<string>();

            clientProvider
                .Received(1)
                .GetClient(options.Value);

            cosmosClient
                .Received(1)
                .GetContainer(
                    options.Value.DatabaseName,
                    providerName);
        }

        [Theory, AutoNSubstituteData]
        public void GetContainer_Of_Unsupported_Type_Throws_NotSupportedException(
            ICosmosClientProvider clientProvider,
            [Substitute] CosmosClient cosmosClient,
            OptionsWrapper<CosmosOptions> options,
            [Substitute] ICosmosContainerNameProvider nameProvider)
        {
            clientProvider
                .GetClient(options.Value)
                .Returns(cosmosClient);
            nameProvider
                .IsForType(typeof(CosmosContainerProviderTests))
                .Returns(false);
            var containerRegistry = new CosmosContainerRegistry(options, new[] { nameProvider });

            var sut = new CosmosContainerProvider(
                clientProvider,
                containerRegistry);

            new Action(() => sut.GetContainer<CosmosContainerProviderTests>())
                .Should()
                .ThrowExactly<NotSupportedException>();

            cosmosClient
                .DidNotReceive()
                .GetContainer(Arg.Any<string>(), Arg.Any<string>());
        }

        [Theory, AutoNSubstituteData]
        public void GetContainer_For_Bulk_Return_Specified_Container(
            ICosmosClientProvider clientProvider,
            [Substitute] CosmosClient cosmosClient,
            OptionsWrapper<CosmosOptions> options,
            [Substitute] Container container,
            [Substitute] ICosmosContainerRegistry containerRegistry,
            string containerName)
        {
            clientProvider
                .GetBulkClient(options.Value)
                .Returns(cosmosClient);
            cosmosClient
                .GetContainer(default, default)
                .ReturnsForAnyArgs(container);
            containerRegistry
                .DefaultOptions
                .Returns(options.Value);

            var sut = new CosmosContainerProvider(
                clientProvider,
                containerRegistry);

            sut.GetContainer(containerName, allowBulk: true)
                .Should()
                .Be(container);

            clientProvider
                .Received(1)
                .GetBulkClient(options.Value);

            cosmosClient
                .Received(1)
                .GetContainer(
                    options.Value.DatabaseName,
                    containerName);
        }

        [Theory, AutoNSubstituteData]
        public void GetContainer_For_Bulk_Returns_NamedContainer(
            ICosmosClientProvider clientProvider,
            [Substitute] CosmosClient cosmosClient,
            OptionsWrapper<CosmosOptions> options,
            ICosmosContainerNameProvider provider,
            [Substitute] Container container,
            string providerName)
        {
            clientProvider
                .GetBulkClient(options.Value)
                .Returns(cosmosClient);
            cosmosClient
                .GetContainer(default, default)
                .ReturnsForAnyArgs(container);
            provider
                .IsForType(typeof(string))
                .Returns(true);
            provider
                .ContainerName
                .Returns(providerName);
            provider
                .Options
                .Returns(options.Value);
            var containerRegistry = new CosmosContainerRegistry(options, new[] { provider });

            var sut = new CosmosContainerProvider(
                clientProvider,
                containerRegistry);

            sut.GetContainer<string>(allowBulk: true)
                .Should()
                .Be(container);

            clientProvider
                .Received(1)
                .GetBulkClient(options.Value);

            cosmosClient
                .Received(1)
                .GetContainer(
                    options.Value.DatabaseName,
                    providerName);
        }

        [Theory, AutoNSubstituteData]
        public void GetContainer_For_Bulk_Of_Unsupported_Type_Throws_NotSupportedException(
            ICosmosClientProvider clientProvider,
            [Substitute] CosmosClient cosmosClient,
            OptionsWrapper<CosmosOptions> options,
            [Substitute] ICosmosContainerNameProvider nameProvider)
        {
            clientProvider
                .GetBulkClient(options.Value)
                .Returns(cosmosClient);
            nameProvider
                .IsForType(typeof(CosmosContainerProviderTests))
                .Returns(false);
            var containerRegistry = new CosmosContainerRegistry(options, new[] { nameProvider });

            var sut = new CosmosContainerProvider(
                clientProvider,
                containerRegistry);
            new Action(() => sut.GetContainer<CosmosContainerProviderTests>(allowBulk: true))
                .Should()
                .ThrowExactly<NotSupportedException>();

            cosmosClient
                .DidNotReceive()
                .GetContainer(Arg.Any<string>(), Arg.Any<string>());
        }
    }
}