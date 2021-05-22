using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Atc.Cosmos.Testing;
using Atc.Test;
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
                .Throw<CosmosException>()
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