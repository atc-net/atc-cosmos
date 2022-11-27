using System;
using System.Linq;
using System.Text;
using Atc.Cosmos.Internal;
using Atc.Cosmos.Serialization;
using Atc.Cosmos.Tests.Generators;
using Atc.Test;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using Microsoft.Win32;
using NSubstitute;
using Xunit;

namespace Atc.Cosmos.Tests.Internal
{

    public class CosmosContainerRegistryTests
    {
        private readonly CosmosOptions cosmosOptions;

        public CosmosContainerRegistryTests()
        {
            var fixture = FixtureFactory.Create();
            cosmosOptions = new CosmosOptions
            {
                AccountEndpoint = fixture.Create<Uri>().AbsoluteUri,
                AccountKey = Convert.ToBase64String(Encoding.UTF8.GetBytes(fixture.Create<string>())),
                DatabaseName = fixture.Create<string>(),
                DatabaseThroughput = fixture.Create<int>(),
            };

        }

        [Fact]
        public void ShouldThrow_When_TokenCredential_And_AccountKey_IsMissing()
        {
            cosmosOptions.Credential = null;
            cosmosOptions.AccountKey = string.Empty;

            FluentActions.Invoking(
                () => new CosmosContainerRegistry(
                    Options.Create(cosmosOptions),
                    Enumerable.Empty<ICosmosContainerNameProvider>()))
                .Should()
                .Throw<InvalidOperationException>();
        }

        [Fact]
        public void ShouldThrow_When_No_AccountEndpoint_IsConfigured()
        {
            cosmosOptions.AccountEndpoint = string.Empty;

            FluentActions.Invoking(
                () => new CosmosContainerRegistry(
                    Options.Create(cosmosOptions),
                    Enumerable.Empty<ICosmosContainerNameProvider>()))
                .Should()
                .Throw<InvalidOperationException>();
        }

        [Fact]
        public void ShouldThrow_When_No_DatabaseName_IsConfigured()
        {
            cosmosOptions.DatabaseName = string.Empty;

            FluentActions.Invoking(
                () => new CosmosContainerRegistry(
                    Options.Create(cosmosOptions),
                    Enumerable.Empty<ICosmosContainerNameProvider>()))
                .Should()
                .Throw<InvalidOperationException>();
        }

        [Theory, AutoNSubstituteData]
        public void GetContainerForType_Of_Unsupported_Type_Throws_NotSupportedException(
            OptionsWrapper<CosmosOptions> options,
            [Substitute] ICosmosContainerNameProvider nameProvider)
        {
            nameProvider
                .IsForType(typeof(CosmosContainerProviderTests))
                .Returns(false);

            var sut = new CosmosContainerRegistry(
                options,
                new[] { nameProvider });

            new Action(() => sut.GetContainerForType<CosmosContainerProviderTests>())
                .Should()
                .ThrowExactly<NotSupportedException>();

            new Action(() => sut.GetContainerForType(typeof(CosmosContainerProviderTests)))
                .Should()
                .ThrowExactly<NotSupportedException>();
        }
    }
}
