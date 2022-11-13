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
            string containerName)
        {
            clientProvider
                .GetClient()
                .Returns(cosmosClient);
            cosmosClient
                .GetContainer(default, default)
                .ReturnsForAnyArgs(container);

            var sut = new CosmosContainerProvider(
                clientProvider,
                new CosmosDatabaseNameProvider(Enumerable.Empty<ICosmosContainerNameProvider>(), options),
                Array.Empty<ICosmosContainerNameProvider>());

            sut.GetContainer(containerName)
                .Should()
                .Be(container);

            clientProvider
                .Received(1)
                .GetClient();

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
            string providerName)
        {
            clientProvider
                .GetClient()
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
                .DatabaseName
                .Returns(options.Value.DatabaseName);

            var sut = new CosmosContainerProvider(
                clientProvider,
                new CosmosDatabaseNameProvider(Enumerable.Empty<ICosmosContainerNameProvider>(), options),
                new[] { provider });

            sut.GetContainer<string>()
                .Should()
                .Be(container);

            clientProvider
                .Received(1)
                .GetClient();

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
                .GetClient()
                .Returns(cosmosClient);
            nameProvider
                .IsForType(typeof(CosmosContainerProviderTests))
                .Returns(false);

            var sut = new CosmosContainerProvider(
                clientProvider,
                new CosmosDatabaseNameProvider(Enumerable.Empty<ICosmosContainerNameProvider>(), options),
                new[] { nameProvider });
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
            string containerName)
        {
            clientProvider
                .GetBulkClient()
                .Returns(cosmosClient);
            cosmosClient
                .GetContainer(default, default)
                .ReturnsForAnyArgs(container);

            var sut = new CosmosContainerProvider(
                clientProvider,
                new CosmosDatabaseNameProvider(Enumerable.Empty<ICosmosContainerNameProvider>(), options),
                Array.Empty<ICosmosContainerNameProvider>());

            sut.GetContainer(containerName, allowBulk: true)
                .Should()
                .Be(container);

            clientProvider
                .Received(1)
                .GetBulkClient();

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
                .GetBulkClient()
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
                .DatabaseName
                .Returns(options.Value.DatabaseName);

            var sut = new CosmosContainerProvider(
                clientProvider,
                new CosmosDatabaseNameProvider(Enumerable.Empty<ICosmosContainerNameProvider>(), options),
                new[] { provider });

            sut.GetContainer<string>(allowBulk: true)
                .Should()
                .Be(container);

            clientProvider
                .Received(1)
                .GetBulkClient();

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
                .GetBulkClient()
                .Returns(cosmosClient);
            nameProvider
                .IsForType(typeof(CosmosContainerProviderTests))
                .Returns(false);

            var sut = new CosmosContainerProvider(
                clientProvider,
                new CosmosDatabaseNameProvider(Enumerable.Empty<ICosmosContainerNameProvider>(), options),
                new[] { nameProvider });
            new Action(() => sut.GetContainer<CosmosContainerProviderTests>(allowBulk: true))
                .Should()
                .ThrowExactly<NotSupportedException>();

            cosmosClient
                .DidNotReceive()
                .GetContainer(Arg.Any<string>(), Arg.Any<string>());
        }
    }
}