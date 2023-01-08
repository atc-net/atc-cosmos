using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Atc.Cosmos.Testing;
using Atc.Test;
using AutoFixture;
using AutoFixture.Xunit2;
using Dasync.Collections;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Xunit;

namespace Atc.Cosmos.Tests.Testing
{
    public class FakeCosmosReaderTests
    {
        [Theory, AutoNSubstituteData]
        public async Task FindAsync_Should_Return_Document_When_Exists(
            FakeCosmosReader<Record> sut,
            Record record)
        {
            sut.Documents.Add(record);

            var result = await sut.FindAsync(
                record.Id,
                record.Pk);

            result
                .Should()
                .BeEquivalentTo(
                    record);
        }

        [Theory, AutoNSubstituteData]
        public async Task FindAsync_Should_Return_Null_Not_Exists(
            FakeCosmosReader<Record> sut,
            Record record)
        {
            var result = await sut.FindAsync(
                record.Id,
                record.Pk);

            result
                .Should()
                .BeNull();
        }

        [Theory, AutoNSubstituteData]
        public async Task ReadAsync_Should_Return_Document_When_Exists(
            FakeCosmosReader<Record> sut,
            Record record)
        {
            sut.Documents.Add(record);

            var result = await sut.ReadAsync(
                record.Id,
                record.Pk);

            result
                .Should()
                .BeEquivalentTo(
                    record);
        }

        [Theory, AutoNSubstituteData]
        public void ReadAsync_Should_Throw_When_Not_Exists(
            FakeCosmosReader<Record> sut,
            string documentId,
            string partitionKey)
        {
            FluentActions.Awaiting(() => sut.ReadAsync(documentId, partitionKey))
                .Should()
                .ThrowAsync<CosmosException>()
                .Where(e => e.StatusCode == System.Net.HttpStatusCode.NotFound);
        }

        [Theory, AutoNSubstituteData]
        public async Task ReadAllAsync_Should_Return_All_Documents_With_PartitionKey(
            FakeCosmosReader<Record> sut,
            string partitionKey)
        {
            sut.Documents.ForEach(d => d.Pk = partitionKey);

            var results = await sut
                .ReadAllAsync(partitionKey)
                .ToListAsync();

            results
                .Should()
                .BeEquivalentTo(sut.Documents);
        }

        [Theory, AutoNSubstituteData]
        public async Task ReadAllAsync_Should_Not_Return_Documents_With_Different_PartitionKey(
            FakeCosmosReader<Record> sut,
            string partitionKey)
        {
            var results = await sut
                .ReadAllAsync(partitionKey)
                .ToListAsync();

            results
                .Should()
                .BeEmpty();
        }

        [Theory, AutoNSubstituteData]
        public async Task QueryAsync_Should_Return_All_Documents_With_PartitionKey(
            FakeCosmosReader<Record> sut,
            QueryDefinition query,
            Record[] queryResults,
            string partitionKey)
        {
            sut.QueryResults.AddRange(queryResults);

            var results = await sut
                .QueryAsync(
                    query,
                    partitionKey)
                .ToListAsync();

            results
                .Should()
                .BeEquivalentTo(queryResults);
        }

        [Theory, AutoNSubstituteData]
        public async Task QueryAsync_Should_Return_All_Documents_With_PartitionKey_When_Given_CatchAll_Query(
            FakeCosmosReader<Record> sut,
            Record[] recordsForQuery,
            string partitionKey)
        {
            sut.Documents.AddRange(recordsForQuery);
            sut.Documents.ForEach(d => d.Pk = partitionKey);

            var results = await sut
                .QueryAsync(
                    x => x.Where(_ => true),
                    partitionKey)
                .ToListAsync();

            results
                .Should()
                .BeEquivalentTo(sut.Documents);
        }

        [Theory, AutoNSubstituteData]
        public async Task QueryAsync_Should_Return_No_Documents_With_Unused_PartitionKey(
            FakeCosmosReader<Record> sut,
            Record[] recordsForQuery,
            string partitionKey)
        {
            sut.Documents.AddRange(recordsForQuery);

            var results = await sut
                .QueryAsync(
                    x => x.Where(_ => true),
                    partitionKey)
                .ToListAsync();

            results
                .Should()
                .BeEmpty();
        }

        [Theory, AutoNSubstituteData]
        public async Task QueryAsync_Should_Return_No_Documents_When_Given_CatchNone_Query(
            FakeCosmosReader<Record> sut,
            Record[] recordsForQuery,
            string partitionKey)
        {
            sut.Documents.AddRange(recordsForQuery);
            sut.Documents.ForEach(d => d.Pk = partitionKey);

            var results = await sut
                .QueryAsync(
                    x => x.Where(_ => false),
                    partitionKey)
                .ToListAsync();

            results
                .Should()
                .BeEmpty();
        }

        [Theory, AutoNSubstituteData]
        public async Task QueryAsync_Of_T_Should_Return_All_QueryResults_Of_Requested_Type(
            FakeCosmosReader<Record> sut,
            QueryDefinition query,
            string partitionKey,
            List<RecordAggregate> queryResults)
        {
            sut.QueryResults = queryResults.Cast<object>().ToList();

            var results = await sut
                .QueryAsync<RecordAggregate>(
                    query,
                    partitionKey)
                .ToListAsync();

            results
                .Should()
                .BeEquivalentTo(queryResults);
        }

        [Theory, AutoNSubstituteData]
        public async Task QueryAsync_Of_T_Should_Not_Return_QueryResults_Of_Wrong_Type(
            FakeCosmosReader<Record> sut,
            QueryDefinition query,
            string partitionKey,
            List<object> queryResults)
        {
            sut.QueryResults = queryResults;

            var results = await sut
                .QueryAsync<RecordAggregate>(
                    query,
                    partitionKey)
                .ToListAsync();

            results
                .Should()
                .BeEmpty();
        }

        [Theory, AutoNSubstituteData]
        public async Task PagedQueryAsync_Should_Return_Result_In_Pages(
            FakeCosmosReader<Record> sut,
            QueryDefinition query,
            Record[] queryResults,
            string partitionKey)
        {
            sut.QueryResults.AddRange(queryResults);

            var page1 = await sut
                .PagedQueryAsync(
                    query,
                    partitionKey,
                    1,
                    null);
            var page2 = await sut
                .PagedQueryAsync(
                    query,
                    partitionKey,
                    1,
                    page1.ContinuationToken);

            page1.Items
                .Should()
                .BeEquivalentTo(new[] { queryResults[0] });
            page2.Items
                .Should()
                .BeEquivalentTo(new[] { queryResults[1] });
        }

        [Theory, AutoNSubstituteData]
        public async Task PagedQueryAsync_With_LINQ_Should_Return_Result_In_Pages(
            FakeCosmosReader<Record> sut,
            Record[] recordsForQuery,
            string partitionKey)
        {
            sut.Documents.Clear();
            sut.Documents.AddRange(recordsForQuery);
            sut.Documents.ForEach(x => x.Pk = partitionKey);

            var page1 = await sut
                .PagedQueryAsync(
                    x => x.Where(_ => true),
                    partitionKey,
                    1,
                    null);
            var page2 = await sut
                .PagedQueryAsync(
                    x => x.Where(_ => true),
                    partitionKey,
                    1,
                    page1.ContinuationToken);

            page1.Items
                .Should()
                .BeEquivalentTo(new[] { recordsForQuery[0] });
            page2.Items
                .Should()
                .BeEquivalentTo(new[] { recordsForQuery[1] });
        }

        [Theory, AutoNSubstituteData]
        public async Task PagedQueryAsync_Of_T_Should_Return_Result_In_Pages(
            FakeCosmosReader<Record> sut,
            QueryDefinition query,
            RecordAggregate[] queryResults,
            string partitionKey)
        {
            sut.QueryResults.AddRange(queryResults);

            var page1 = await sut
                .PagedQueryAsync<RecordAggregate>(
                    query,
                    partitionKey,
                    1,
                    null);
            var page2 = await sut
                .PagedQueryAsync<RecordAggregate>(
                    query,
                    partitionKey,
                    1,
                    page1.ContinuationToken);

            page1.Items
                .Should()
                .BeEquivalentTo(new[] { queryResults[0] });
            page2.Items
                .Should()
                .BeEquivalentTo(new[] { queryResults[1] });
        }

        [Theory, AutoNSubstituteData]
        public async Task CrossPartitionQuery_Should_Return_All_Documents_When_Given_CatchAll_Query(FakeCosmosReader<Record> sut)
        {
            var result = await sut.CrossPartitionQueryAsync(x => x.Where(_ => true)).ToListAsync();
            result.Should().BeEquivalentTo(sut.Documents);
        }

        [Theory, AutoNSubstituteData]
        public async Task PagedCrossPartitionQuery_Should_Return_All_Documents_When_Given_CatchAll_Query(FakeCosmosReader<Record> sut)
        {
            var requiredDocuments = new HashSet<Record>(sut.Documents);
            string? continuationToken = null;

            while (requiredDocuments.Any())
            {
                var result = await sut.CrossPartitionPagedQueryAsync(x => x.Where(_ => true), 1, continuationToken);
                continuationToken = result.ContinuationToken;
                requiredDocuments.Should().Contain(result.Items);
                requiredDocuments.Remove(result.Items[0]);
            }
        }

        [Theory, AutoNSubstituteData]
        public async Task BatchQuery_Should_Return_All_Documents_When_Given_CatchAll_Query(
            FakeCosmosReader<Record> sut,
            Record[] recordsForQuery,
            string partitionKey)
        {
            sut.Documents.AddRange(recordsForQuery);
            sut.Documents.ForEach(x => x.Pk = partitionKey);

            var result = await sut.BatchQueryAsync(x => x.Where(_ => true), partitionKey).ToListAsync();

            result[0].Should().HaveCount(3); // Fake implementation of BatchQueryAsync will return batches of size 3
            result[0].Should().NotBeEquivalentTo(recordsForQuery); // First 3 will be those already in sut.Documents before we inserted our own
            result[1].Should().HaveCount(3);
            result[1].Should().BeEquivalentTo(recordsForQuery); // The next 3 should be those query results that we inserted
        }

        [Theory, AutoNSubstituteData]
        public async Task BatchCrossPartitionQuery_Should_Return_All_Documents_When_Given_CatchAll_Query(
            FakeCosmosReader<Record> sut,
            Record[] recordsForQuery)
        {
            sut.Documents.AddRange(recordsForQuery);

            var result = await sut.BatchCrossPartitionQueryAsync(x => x.Where(_ => true)).ToListAsync();

            result[0].Should().HaveCount(3); // Fake implementation of BatchCrossPartitionQueryAsync will return batches of size 3
            result[0].Should().NotBeEquivalentTo(recordsForQuery); // First 3 will be those already in sut.Documents before we inserted our own
            result[1].Should().HaveCount(3);
            result[1].Should().BeEquivalentTo(recordsForQuery); // The next 3 should be those query results that we inserted
        }

        [Theory, AutoNSubstituteData]
        public void Should_Be_Able_To_Inject_As_Frozen_CosmosReader(
            [Frozen(Matching.ImplementedInterfaces)]
            FakeCosmosReader<Record> sut,
            TestCosmosService<Record> service)
        {
            service.Reader.Should().BeSameAs(sut);
        }

        [Theory, AutoNSubstituteData]
        public void Should_Be_Able_To_Inject_As_Frozen_CosmosBulkReader(
            [Frozen(Matching.ImplementedInterfaces)]
            FakeCosmosReader<Record> sut,
            TestCosmosService<Record> service)
        {
            service.BulkReader.Should().BeSameAs(sut);
        }
    }
}