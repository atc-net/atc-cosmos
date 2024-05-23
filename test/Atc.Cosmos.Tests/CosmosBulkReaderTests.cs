using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Atc.Cosmos.Internal;
using Atc.Test;
using AutoFixture;
using Dasync.Collections;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using NSubstitute;
using Xunit;

namespace Atc.Cosmos.Tests
{
    public class CosmosBulkReaderTests
    {
        private readonly ItemResponse<Record> itemResponse;
        private readonly FeedIterator<Record> feedIterator;
        private readonly FeedResponse<Record> feedResponse;
        private readonly Record record;
        private readonly Container container;
        private readonly ICosmosContainerProvider containerProvider;
        private readonly CosmosBulkReader<Record> sut;

        public CosmosBulkReaderTests()
        {
            record = new Fixture().Create<Record>();
            itemResponse = Substitute.For<ItemResponse<Record>>();
            itemResponse
                .Resource
                .Returns(record);

            feedResponse = Substitute.For<FeedResponse<Record>>();
            feedIterator = Substitute.For<FeedIterator<Record>>();
            feedIterator
                .ReadNextAsync(default)
                .ReturnsForAnyArgs(feedResponse);

            container = Substitute.For<Container>();
            container
                .ReadItemAsync<Record>(default, default, default)
                .ReturnsForAnyArgs(itemResponse);

            container
                .GetItemQueryIterator<Record>(default(QueryDefinition), default)
                .ReturnsForAnyArgs(feedIterator);

            container
                .GetItemQueryIterator<Record>(default(string), default)
                .ReturnsForAnyArgs(feedIterator);

            containerProvider = Substitute.For<ICosmosContainerProvider>();
            containerProvider
                .GetContainer<Record>(allowBulk: true)
                .Returns(container, null);
            sut = new CosmosBulkReader<Record>(containerProvider);
        }

        [Fact]
        public void Implements_Interface()
            => sut.Should().BeAssignableTo<ICosmosBulkReader<Record>>();

        [Theory, AutoNSubstituteData]
        public async Task ReadAsync_Uses_The_Right_Container(
            string documentId,
            string partitionKey,
            CancellationToken cancellationToken)
        {
            await sut.ReadAsync(documentId, partitionKey, cancellationToken);

            containerProvider
                .Received(1)
                .GetContainer<Record>(allowBulk: true);
        }

        [Theory, AutoNSubstituteData]
        public async Task ReadAsync_Reads_Item_In_Container(
            string documentId,
            string partitionKey,
            CancellationToken cancellationToken)
        {
            await sut.ReadAsync(documentId, partitionKey, cancellationToken);

            _ = container
                .Received(1)
                .ReadItemAsync<Record>(
                    documentId,
                    new PartitionKey(partitionKey),
#if PREVIEW
                    Arg.Is<ItemRequestOptions>(c => c.PriorityLevel == PriorityLevel.High),
#else
                    Arg.Any<ItemRequestOptions>(),
#endif
                    cancellationToken);
        }

        [Theory, AutoNSubstituteData]
        public async Task ReadAsync_Returns_Item_Read_From_Container(
            string documentId,
            string partitionKey,
            CancellationToken cancellationToken)
        {
            var result = await sut.ReadAsync(documentId, partitionKey, cancellationToken);
            result
                .Should()
                .Be(itemResponse.Resource);
        }

        [Theory, AutoNSubstituteData]
        public void ReadAsync_Throws_Expection_When_Record_IsNot_Found(
            CosmosException exception,
            string documentId,
            string partitionKey,
            CancellationToken cancellationToken)
        {
            container
                .ReadItemAsync<Record>(default, default, default, default)
                .ReturnsForAnyArgs(Task.FromException<ItemResponse<Record>>(exception));

            FluentActions
                .Awaiting(() => sut.ReadAsync(documentId, partitionKey, cancellationToken))
                .Should()
                .ThrowAsync<CosmosException>();
        }

        [Theory, AutoNSubstituteData]
        public async Task FindAsync_Uses_The_Right_Container(
            string documentId,
            string partitionKey,
            CancellationToken cancellationToken)
        {
            await sut.FindAsync(documentId, partitionKey, cancellationToken);

            containerProvider
                .Received(1)
                .GetContainer<Record>(allowBulk: true);
        }

        [Theory, AutoNSubstituteData]
        public async Task FindAsync_Return_Default_When_Record_IsNot_Found(
            CosmosException exception,
            string documentId,
            string partitionKey,
            CancellationToken cancellationToken)
        {
            container
                .ReadItemAsync<Record>(default, default, default, default)
                .ReturnsForAnyArgs(Task.FromException<ItemResponse<Record>>(exception));

            var response = await sut.FindAsync(documentId, partitionKey, cancellationToken);

            response
                .Should()
                .BeNull();
        }

        [Theory, AutoNSubstituteData]
        public async Task FindAsync_Returns_Record_When_Successful(
            string partitionKey,
            string documentId,
            CancellationToken cancellationToken)
        {
            var result = await sut.FindAsync(documentId, partitionKey, cancellationToken);
            result
                .Should()
                .Be(record);
        }

        [Theory, AutoNSubstituteData]
        public void ReadAllAsync_Uses_The_Right_Container(
            string partitionKey,
            CancellationToken cancellationToken)
        {
            _ = sut.ReadAllAsync(partitionKey, cancellationToken);

            containerProvider
                .Received(1)
                .GetContainer<Record>(allowBulk: true);
        }

        [Theory, AutoNSubstituteData]
        public async Task ReadAllAsync_Returns_Empty_No_More_Result(
            string partitionKey,
            CancellationToken cancellationToken)
        {
            feedIterator.HasMoreResults.Returns(false);

            var response = await sut
                .ReadAllAsync(partitionKey, cancellationToken)
                .ToListAsync(cancellationToken);

            _ = feedIterator
                .Received(1)
                .HasMoreResults;

            _ = feedIterator
                .Received(0)
                .ReadNextAsync(default);

            response
                .Should()
                .BeEmpty();
        }

        [Theory, AutoNSubstituteData]
        public async Task ReadAllAsync_Returns_Empty_When_Query_Matches_Non(
            string partitionKey,
            CancellationToken cancellationToken)
        {
            feedIterator.HasMoreResults.Returns(true, false);

            var response = await sut
                .ReadAllAsync(partitionKey, cancellationToken)
                .ToListAsync(cancellationToken);

            _ = feedIterator
                .Received(2)
                .HasMoreResults;

            _ = feedIterator
                .Received(1)
                .ReadNextAsync(default);

            response
                .Should()
                .BeEmpty();
        }

        [Theory, AutoNSubstituteData]
        public async Task ReadAllAsync_Returns_All_Items(
            string partitionKey,
            CancellationToken cancellationToken)
        {
            feedIterator
                .HasMoreResults
                .Returns(true, false);

            feedResponse
                .GetEnumerator()
                .Returns(new List<Record> { record }.GetEnumerator());

            var response = await sut
                .ReadAllAsync(partitionKey, cancellationToken)
                .ToListAsync(cancellationToken);

            _ = feedIterator
                .Received(2)
                .HasMoreResults;

            _ = feedIterator
                .Received(1)
                .ReadNextAsync(default);

            response
                .Should()
                .NotBeEmpty();

            response[0]
                .Should()
                .Be(record);
        }

        [Theory, AutoNSubstituteData]
        public void QueryAsync_Uses_The_Right_Container(
            QueryDefinition query,
            string partitionKey,
            CancellationToken cancellationToken)
        {
            _ = sut.QueryAsync(query, partitionKey, cancellationToken);

            containerProvider
                .Received(1)
                .GetContainer<Record>(allowBulk: true);
        }

        [Theory, AutoNSubstituteData]
        public async Task QueryAsync_Returns_Empty_No_More_Result(
            QueryDefinition query,
            string partitionKey,
            CancellationToken cancellationToken)
        {
            feedIterator.HasMoreResults.Returns(false);

            var response = await sut.QueryAsync(query, partitionKey, cancellationToken).ToListAsync(cancellationToken);

            _ = feedIterator
                .Received(1)
                .HasMoreResults;

            _ = feedIterator
                .Received(0)
                .ReadNextAsync(default);

            response
                .Should()
                .BeEmpty();
        }

        [Theory, AutoNSubstituteData]
        public async Task QueryAsync_Returns_Empty_When_Query_Matches_Non(
            QueryDefinition query,
            string partitionKey,
            CancellationToken cancellationToken)
        {
            feedIterator.HasMoreResults.Returns(true, false);

            var response = await sut.QueryAsync(query, partitionKey, cancellationToken).ToListAsync(cancellationToken);

            _ = feedIterator
                .Received(2)
                .HasMoreResults;

            _ = feedIterator
                .Received(1)
                .ReadNextAsync(default);

            response
                .Should()
                .BeEmpty();
        }

        [Theory, AutoNSubstituteData]
        public async Task QueryAsync_Returns_Items_When_Query_Matches(
            QueryDefinition query,
            string partitionKey,
            CancellationToken cancellationToken)
        {
            feedIterator
                .HasMoreResults
                .Returns(true, false);

            feedResponse
                .GetEnumerator()
                .Returns(new List<Record> { record }.GetEnumerator());

            var response = await sut.QueryAsync(query, partitionKey, cancellationToken).ToListAsync(cancellationToken);

            _ = feedIterator
                .Received(2)
                .HasMoreResults;

            _ = feedIterator
                .Received(1)
                .ReadNextAsync(default);

            response
                .Should()
                .NotBeEmpty();

            response[0]
                .Should()
                .Be(record);
        }

        [Theory, AutoNSubstituteData]
        public async Task Should_Have_ETag_From_ItemResponse(
            string etag,
            string partitionKey,
            string documentId,
            CancellationToken cancellationToken)
        {
            itemResponse
                .ETag
                .Returns(etag);
            itemResponse
                .Resource
                .Returns(record);

            var result = await sut.FindAsync(documentId, partitionKey, cancellationToken);

            var resource = result as ICosmosResource;
            resource
                .Should()
                .NotBeNull();

            resource
                .ETag
                .Should()
                .NotBeNullOrWhiteSpace();

            resource
                .ETag
                .Should()
                .Be(etag);
        }

        [Theory, AutoNSubstituteData]
        public void Multiple_Operations_Uses_Same_Container(
            QueryDefinition query,
            string documentId,
            string partitionKey,
            CancellationToken cancellationToken)
        {
            _ = sut.ReadAsync(documentId, partitionKey, cancellationToken);
            _ = sut.ReadAsync(documentId, partitionKey, cancellationToken);
            _ = sut.FindAsync(documentId, partitionKey, cancellationToken);
            _ = sut.FindAsync(documentId, partitionKey, cancellationToken);
            _ = sut.QueryAsync(query, partitionKey, cancellationToken).ToListAsync(cancellationToken);
            _ = sut.QueryAsync(query, partitionKey, cancellationToken).ToListAsync(cancellationToken);

            container
                .ReceivedCalls()
                .Should()
                .HaveCount(6);
        }

        [Theory, AutoNSubstituteData]
        public void QueryAsync_With_Custom_Result_Uses_The_Right_Container(
            QueryDefinition query,
            string partitionKey,
            CancellationToken cancellationToken)
        {
            _ = sut.QueryAsync<Record>(query, partitionKey, cancellationToken);

            containerProvider
                .Received(1)
                .GetContainer<Record>(allowBulk: true);
        }

        [Theory, AutoNSubstituteData]
        public async Task QueryAsync_With_Custom_Returns_Empty_No_More_Result(
            QueryDefinition query,
            string partitionKey,
            CancellationToken cancellationToken)
        {
            feedIterator.HasMoreResults.Returns(false);

            var response = await sut.QueryAsync<Record>(query, partitionKey, cancellationToken).ToListAsync(cancellationToken);

            _ = feedIterator
                .Received(1)
                .HasMoreResults;

            _ = feedIterator
                .Received(0)
                .ReadNextAsync(default);

            response
                .Should()
                .BeEmpty();
        }

        [Theory, AutoNSubstituteData]
        public async Task QueryAsync_With_Custom_Returns_Empty_When_Query_Matches_Non(
            QueryDefinition query,
            string partitionKey,
            CancellationToken cancellationToken)
        {
            feedIterator.HasMoreResults.Returns(true, false);

            var response = await sut.QueryAsync<Record>(query, partitionKey, cancellationToken).ToListAsync(cancellationToken);

            _ = feedIterator
                .Received(2)
                .HasMoreResults;

            _ = feedIterator
                .Received(1)
                .ReadNextAsync(default);

            response
                .Should()
                .BeEmpty();
        }

        [Theory, AutoNSubstituteData]
        public async Task QueryAsync_With_Custom_Returns_Items_When_Query_Matches(
            QueryDefinition query,
            string partitionKey,
            CancellationToken cancellationToken)
        {
            feedIterator
                .HasMoreResults
                .Returns(true, false);

            feedResponse
                .GetEnumerator()
                .Returns(new List<Record> { record }.GetEnumerator());

            var response = await sut.QueryAsync<Record>(query, partitionKey, cancellationToken).ToListAsync(cancellationToken);

            _ = feedIterator
                .Received(2)
                .HasMoreResults;

            _ = feedIterator
                .Received(1)
                .ReadNextAsync(default);

            response
                .Should()
                .NotBeEmpty();

            response[0]
                .Should()
                .Be(record);
        }

        [Theory, AutoNSubstituteData]
        public void CrossPartitionQueryAsync_Uses_The_Right_Container(
            QueryDefinition query,
            CancellationToken cancellationToken)
        {
            _ = sut.CrossPartitionQueryAsync(query, cancellationToken);

            containerProvider
                .Received(1)
                .GetContainer<Record>(allowBulk: true);
        }

        [Theory, AutoNSubstituteData]
        public void CrossPartitionQueryAsync_Does_Not_Specify_QueryRequestOptions(
            QueryDefinition query,
            CancellationToken cancellationToken)
        {
            _ = sut.CrossPartitionQueryAsync(query, cancellationToken).ToArrayAsync(cancellationToken);

            container
                .Received(1)
                .GetItemQueryIterator<Record>(query, requestOptions: null);
        }

        [Theory, AutoNSubstituteData]
        public async Task CrossPartitionQueryAsync_Returns_Empty_No_More_Result(
            QueryDefinition query,
            CancellationToken cancellationToken)
        {
            feedIterator.HasMoreResults.Returns(false);

            var response = await sut.CrossPartitionQueryAsync(query, cancellationToken).ToListAsync(cancellationToken);

            _ = feedIterator
                .Received(1)
                .HasMoreResults;

            _ = feedIterator
                .Received(0)
                .ReadNextAsync(default);

            response
                .Should()
                .BeEmpty();
        }

        [Theory, AutoNSubstituteData]
        public async Task CrossPartitionQueryAsync_Returns_Empty_When_Query_Matches_Non(
            QueryDefinition query,
            CancellationToken cancellationToken)
        {
            feedIterator.HasMoreResults.Returns(true, false);

            var response = await sut.CrossPartitionQueryAsync(query, cancellationToken).ToListAsync(cancellationToken);

            _ = feedIterator
                .Received(2)
                .HasMoreResults;

            _ = feedIterator
                .Received(1)
                .ReadNextAsync(default);

            response
                .Should()
                .BeEmpty();
        }

        [Theory, AutoNSubstituteData]
        public async Task CrossPartitionQueryAsync_Returns_Items_When_Query_Matches(
            QueryDefinition query,
            CancellationToken cancellationToken)
        {
            feedIterator
                .HasMoreResults
                .Returns(true, false);

            feedResponse
                .GetEnumerator()
                .Returns(new List<Record> { record }.GetEnumerator());

            var response = await sut.CrossPartitionQueryAsync(query, cancellationToken).ToListAsync(cancellationToken);

            _ = feedIterator
                .Received(2)
                .HasMoreResults;

            _ = feedIterator
                .Received(1)
                .ReadNextAsync(default);

            response
                .Should()
                .NotBeEmpty();

            response[0]
                .Should()
                .Be(record);
        }

        [Theory, AutoNSubstituteData]
        public void CrossPartitionPagedQueryAsync_Uses_The_Right_Container(
            QueryDefinition query,
            int pageSize,
            string continuationToken,
            CancellationToken cancellationToken)
        {
            _ = sut.CrossPartitionPagedQueryAsync(
                query,
                pageSize,
                continuationToken,
                cancellationToken);

            containerProvider
                .Received(1)
                .GetContainer<Record>(allowBulk: true);
        }

        [Theory, AutoNSubstituteData]
        public void CrossPartitionPagedQueryAsync_Gets_ItemQueryIterator(
            QueryDefinition query,
            int pageSize,
            string continuationToken,
            CancellationToken cancellationToken)
        {
            _ = sut.CrossPartitionPagedQueryAsync(
                query,
                pageSize,
                continuationToken,
                cancellationToken);

            container
                .Received(1)
                .GetItemQueryIterator<Record>(
                    query,
                    continuationToken,
                    requestOptions: Arg.Is<QueryRequestOptions>(o
                        => o.PartitionKey == null
                        && o.MaxItemCount == pageSize));
        }

        [Theory, AutoNSubstituteData]
        public async Task CrossPartitionPagedQueryAsync_Returns_Empty_When_No_More_Result(
            QueryDefinition query,
            int pageSize,
            string continuationToken,
            CancellationToken cancellationToken)
        {
            feedIterator.HasMoreResults.Returns(false);

            var response = await sut
                .CrossPartitionPagedQueryAsync(
                    query,
                    pageSize,
                    continuationToken,
                    cancellationToken);

            _ = feedIterator
                .Received(1)
                .HasMoreResults;

            _ = feedIterator
                .Received(0)
                .ReadNextAsync(default);

            response.Items
                .Should()
                .BeEmpty();
            response.ContinuationToken
                .Should()
                .BeNull();
        }

        [Theory, AutoNSubstituteData]
        public async Task CrossPartitionPagedQueryAsync_Returns_Items_When_Query_Matches(
            QueryDefinition query,
            int pageSize,
            string continuationToken,
            List<Record> records,
            CancellationToken cancellationToken)
        {
            feedIterator
                .HasMoreResults
                .Returns(true);
            feedResponse
                .ContinuationToken
                .Returns(continuationToken);
            feedResponse
                .GetEnumerator()
                .Returns(records.GetEnumerator());

            var response = await sut
                .CrossPartitionPagedQueryAsync(
                    query,
                    pageSize,
                    null,
                    cancellationToken);

            _ = feedIterator
                .Received(1)
                .HasMoreResults;

            _ = feedIterator
                .Received(1)
                .ReadNextAsync(default);

            response.Items
                .Should()
                .BeEquivalentTo(records);

            response.ContinuationToken
                .Should()
                .Be(continuationToken);
        }

        [Theory, AutoNSubstituteData]
        public void CrossPartitionPagedQueryAsync_With_Custom_Uses_The_Right_Container(
            QueryDefinition query,
            int pageSize,
            string continuationToken,
            CancellationToken cancellationToken)
        {
            _ = sut.CrossPartitionPagedQueryAsync<Record>(
                query,
                pageSize,
                continuationToken,
                cancellationToken);

            containerProvider
                .Received(1)
                .GetContainer<Record>(allowBulk: true);
        }

        [Theory, AutoNSubstituteData]
        public async Task CrossPartitionPagedQueryAsync_With_Custom_Returns_Empty_No_More_Result(
            QueryDefinition query,
            int pageSize,
            string continuationToken,
            CancellationToken cancellationToken)
        {
            feedIterator.HasMoreResults.Returns(false);

            var response = await sut
                .CrossPartitionPagedQueryAsync<Record>(
                    query,
                    pageSize,
                    continuationToken,
                    cancellationToken);

            _ = feedIterator
                .Received(1)
                .HasMoreResults;

            _ = feedIterator
                .Received(0)
                .ReadNextAsync(default);

            response.Items
                .Should()
                .BeEmpty();
            response.ContinuationToken
                .Should()
                .BeNull();
        }

        [Theory, AutoNSubstituteData]
        public async Task CrossPartitionPagedQueryAsync_With_Custom_Returns_Items_When_Query_Matches(
            QueryDefinition query,
            int pageSize,
            string continuationToken,
            List<Record> records,
            CancellationToken cancellationToken)
        {
            feedIterator
                .HasMoreResults
                .Returns(true);
            feedResponse
                .ContinuationToken
                .Returns(continuationToken);
            feedResponse
                .GetEnumerator()
                .Returns(records.GetEnumerator());

            var response = await sut
                .CrossPartitionPagedQueryAsync<Record>(
                    query,
                    pageSize,
                    null,
                    cancellationToken);

            _ = feedIterator
                .Received(1)
                .HasMoreResults;

            _ = feedIterator
                .Received(1)
                .ReadNextAsync(default);

            response.Items
                .Should()
                .BeEquivalentTo(records);

            response.ContinuationToken
                .Should()
                .Be(continuationToken);
        }
    }
}