using System.Threading;
using System.Threading.Tasks;
using Atc.Cosmos.Internal;
using Atc.Cosmos.Serialization;
using Atc.Test;
using AutoFixture;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using NSubstitute;
using Xunit;

namespace Atc.Cosmos.Tests
{
    public class CosmosBulkWriterTests
    {
        private readonly Record record;
        private readonly Container container;
        private readonly ICosmosContainerProvider containerProvider;
        private readonly IJsonCosmosSerializer serializer;
        private readonly CosmosBulkWriter<Record> sut;

        public CosmosBulkWriterTests()
        {
            record = new Fixture().Create<Record>();

            container = Substitute.For<Container>();

            containerProvider = Substitute.For<ICosmosContainerProvider>();
            containerProvider
                .GetContainer<Record>()
                .ReturnsForAnyArgs(container, null);

            var response = Substitute.For<ItemResponse<object>>();
            response.Resource.Returns(new Fixture().Create<string>());
            container
                .CreateItemAsync<object>(default, default, default, default)
                .ReturnsForAnyArgs(response);
            container
                .ReplaceItemAsync<object>(default, default, default, default, default)
                .ReturnsForAnyArgs(response);
            container
                .UpsertItemAsync<object>(default, default, default, default)
                .ReturnsForAnyArgs(response);

            serializer = Substitute.For<IJsonCosmosSerializer>();
            serializer
                .FromString<Record>(default)
                .ReturnsForAnyArgs(new Fixture().Create<Record>());

            sut = new CosmosBulkWriter<Record>(containerProvider);
        }

        [Fact]
        public void Implements_Interface()
            => sut.Should().BeAssignableTo<ICosmosBulkWriter<Record>>();

        [Theory, AutoNSubstituteData]
        public async Task WriteAsync_Uses_The_Right_Container(
            CancellationToken cancellationToken)
        {
            await sut.WriteAsync(record, cancellationToken);
            containerProvider
                .Received(1)
                .GetContainer<Record>(
                    allowBulk: true);
        }

        [Theory, AutoNSubstituteData]
        public async Task WriteAsync_UpsertItem_In_Container(
            CancellationToken cancellationToken)
        {
            containerProvider
                .GetContainer<Record>()
                .ReturnsForAnyArgs(container);

            await sut.WriteAsync(record, cancellationToken);
            await container
                .Received(1)
                .UpsertItemAsync<object>(
                    record,
                    new PartitionKey(record.Pk),
                    Arg.Is<ItemRequestOptions>(o => o.EnableContentResponseOnWrite == false),
                    cancellationToken);
        }

        [Theory, AutoNSubstituteData]
        public async Task CreateAsync_Calls_CreateItem_On_Container(
           CancellationToken cancellationToken)
        {
            await sut.CreateAsync(record, cancellationToken);
            _ = container
                .Received(1)
                .CreateItemAsync<object>(
                    record,
                    new PartitionKey(record.Pk),
                    Arg.Is<ItemRequestOptions>(o => o.EnableContentResponseOnWrite == false),
                    cancellationToken);
        }

        [Theory, AutoNSubstituteData]
        public async Task ReplaceAsync_Calls_ReplaceItemAsync_On_Container(
           CancellationToken cancellationToken)
        {
            await sut.ReplaceAsync(record, cancellationToken);
            _ = container
                .Received(1)
                .ReplaceItemAsync<object>(
                    record,
                    record.Id,
                    new PartitionKey(record.Pk),
                    Arg.Is<ItemRequestOptions>(o =>
                        o.EnableContentResponseOnWrite == false &&
                        o.IfMatchEtag == ((ICosmosResource)record).ETag),
                    cancellationToken);
        }

        [Theory, AutoNSubstituteData]
        public void Multiple_Operations_Uses_Same_Container(
            CancellationToken cancellationToken)
        {
            _ = sut.WriteAsync(record, cancellationToken);
            _ = sut.WriteAsync(record, cancellationToken);
            _ = sut.CreateAsync(record, cancellationToken);
            _ = sut.CreateAsync(record, cancellationToken);
            _ = sut.ReplaceAsync(record, cancellationToken);
            _ = sut.ReplaceAsync(record, cancellationToken);

            container
                .ReceivedCalls()
                .Should()
                .HaveCount(6);
        }

        [Theory, AutoNSubstituteData]
        public async Task DeleteAsync_Calls_DeleteItemAsync_On_Container(
           CancellationToken cancellationToken)
        {
            await sut.DeleteAsync(record.Id, record.Pk, cancellationToken);
            _ = container
                .Received(1)
                .DeleteItemAsync<object>(
                    record.Id,
                    new PartitionKey(record.Pk),
                    Arg.Is<ItemRequestOptions>(o => o.EnableContentResponseOnWrite == false),
                    cancellationToken: cancellationToken);
        }
    }
}