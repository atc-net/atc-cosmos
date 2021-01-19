using System;
using Atc.Cosmos.Internal;
using Atc.Test;
using AutoFixture.AutoNSubstitute;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace Atc.Cosmos.Tests.Internal
{
    public class CosmosContainerProviderTests
    {
        [Theory, AutoNSubstituteData]
        public void GetContainer_Return_NamedContainer(
            [Substitute] CosmosClient cosmosClient,
            OptionsWrapper<CosmosOptions> options,
            ICosmosContainerNameProvider provider,
            [Substitute] Container container,
            string providerName)
        {
            cosmosClient
                .GetContainer(default, default)
                .ReturnsForAnyArgs(container);

            provider
                .ContainerName
                .Returns(providerName);
            provider
                .FromType
                .Returns(typeof(string));

            var sut = new CosmosContainerProvider(cosmosClient, options, new[] { provider });

            sut.GetContainer<string>()
                .Should()
                .Be(container);

            cosmosClient
                .Received(1)
                .GetContainer(
                    options.Value.DatabaseName,
                    providerName);
        }

        [Theory, AutoNSubstituteData]
        public void GetContainer_Of_Unsupported_Type_Throws_NotSupportedException(
            [Substitute] CosmosClient cosmosClient,
            OptionsWrapper<CosmosOptions> options,
            [Substitute] ICosmosContainerNameProvider provider,
            string providerName)
        {
            provider
                .ContainerName
                .Returns(providerName);
            provider
                .FromType
                .Returns(typeof(string));

            var sut = new CosmosContainerProvider(cosmosClient, options, new[] { provider });
            new Action(() => sut.GetContainer<CosmosContainerProviderTests>())
                .Should()
                .ThrowExactly<NotSupportedException>();

            cosmosClient
                .DidNotReceive()
                .GetContainer(Arg.Any<string>(), Arg.Any<string>());
        }
    }
}