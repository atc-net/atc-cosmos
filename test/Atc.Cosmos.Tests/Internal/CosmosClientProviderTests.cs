using System;
using System.Text;
using Atc.Cosmos.Internal;
using Atc.Cosmos.Serialization;
using Atc.Test;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace Atc.Cosmos.Tests.Internal
{
    public sealed class CosmosClientProviderTests : IDisposable
    {
        private readonly CosmosOptions cosmosOptions;
        private readonly CosmosClientOptions cosmosClientOptions;
        private readonly IJsonCosmosSerializer serializer;
        private readonly CosmosClientProvider sut;

        public CosmosClientProviderTests()
        {
            var fixture = FixtureFactory.Create();
            cosmosOptions = new CosmosOptions
            {
                AccountEndpoint = fixture.Create<Uri>().AbsoluteUri,
                AccountKey = Convert.ToBase64String(Encoding.UTF8.GetBytes(fixture.Create<string>())),
                DatabaseName = fixture.Create<string>(),
                DatabaseThroughput = fixture.Create<int>(),
            };

            cosmosClientOptions = new CosmosClientOptions
            {
                ApplicationName = fixture.Create<string>(),
            };

            serializer = Substitute.For<IJsonCosmosSerializer>();

            sut = new CosmosClientProvider(
                Options.Create(cosmosOptions),
                Options.Create(cosmosClientOptions),
                serializer);
        }

        public void Dispose()
            => sut.Dispose();

        [Fact]
        public void Clients_Should_Use_Endpoint_From_CosmosOptions()
            => sut
                .GetClient()
                .Endpoint
                .Should()
                .BeEquivalentTo(
                    new Uri(cosmosOptions.AccountEndpoint));

        [Fact]
        public void Client_Should_Use_CosmosClientOptions()
            => sut
                .GetClient()
                .ClientOptions
                .Should()
                .BeEquivalentTo(
                    cosmosClientOptions,
                    o => o
                        .Excluding(o => o.AllowBulkExecution)
                        .Excluding(o => o.Serializer));

        [Fact]
        public void Client_Should_Use_Default_Serializer_If_None_Specified()
        {
            cosmosClientOptions.Serializer = null;

            var actualSerializer = sut
                .GetClient()
                .ClientOptions
                .Serializer;

            actualSerializer
                .Should()
                .BeAssignableTo<CosmosSerializerAdapter>();

            ((CosmosSerializerAdapter)actualSerializer)
               .Serializer
               .Should()
               .Be(serializer);
        }

        [Theory, AutoNSubstituteData]
        public void Client_Should_Use_CustomSerializer_If_Specified(
            [Substitute] CosmosSerializer serializer)
        {
            cosmosClientOptions.Serializer = serializer;
            sut
                .GetClient()
                .ClientOptions
                .Serializer
                .Should()
                .Be(serializer);
        }

        [Fact]
        public void GetClient_Should_Return_Same_Instance()
            => sut
                .GetClient()
                .Should()
                .Be(sut.GetClient());

        [Fact]
        public void GetClient_And_GetBulkClinet_Should_Return_Different_Instances()
            => sut
                .GetClient()
                .Should()
                .NotBe(sut.GetBulkClient());

        [Fact]
        public void GetBulkClient_Should_Return_Same_Instance()
            => sut
                .GetBulkClient()
                .Should()
                .Be(sut.GetBulkClient());

        [Fact]
        public void BulkClient_Should_Use_Endpoint_From_CosmosOptions()
            => sut
                .GetBulkClient()
                .Endpoint
                .Should()
                .BeEquivalentTo(
                    new Uri(cosmosOptions.AccountEndpoint));

        [Fact]
        public void BulkClient_Should_Use_CosmosClientOptions()
            => sut
                .GetBulkClient()
                .ClientOptions
                .Should()
                .BeEquivalentTo(
                    cosmosClientOptions,
                    o => o
                        .Excluding(o => o.AllowBulkExecution)
                        .Excluding(o => o.Serializer));

        [Fact]
        public void BulkClient_Should_Use_Default_Serializer_If_None_Specified()
        {
            cosmosClientOptions.Serializer = null;

            var actualSerializer = sut
                .GetBulkClient()
                .ClientOptions
                .Serializer;

            actualSerializer
                .Should()
                .BeAssignableTo<CosmosSerializerAdapter>();

            ((CosmosSerializerAdapter)actualSerializer)
               .Serializer
               .Should()
               .Be(serializer);
        }

        [Theory, AutoNSubstituteData]
        public void BulkClient_Should_Use_CustomSerializer_If_Specified(
            [Substitute] CosmosSerializer serializer)
        {
            cosmosClientOptions.Serializer = serializer;
            sut
                .GetBulkClient()
                .ClientOptions
                .Serializer
                .Should()
                .Be(serializer);
        }
    }
}
