using System;
using System.Threading.Tasks;
using Atc.Cosmos.Testing;
using Atc.Test;
using AutoFixture.AutoNSubstitute;
using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using NSubstitute;
using Xunit;

namespace Atc.Cosmos.Tests.Testing
{
    public class FakeCosmosTests
    {
        [Theory, AutoNSubstituteData]
        public void Should_Have_Reader(
            FakeCosmos<Record> sut)
        {
            sut.Reader.Should().NotBeNull();
            sut.Reader.Documents.Should().BeSameAs(sut.Documents);
            sut.Reader.QueryResults.Should().BeSameAs(sut.QueryResults);
        }

        [Theory, AutoNSubstituteData]
        public void Should_Have_Writer(
            FakeCosmos<Record> sut)
        {
            sut.Writer.Should().NotBeNull();
            sut.Writer.Documents.Should().BeSameAs(sut.Documents);
        }

        [Theory, AutoNSubstituteData]
        public void Should_Be_Able_To_Inject_As_Frozen_CosmosReader(
            [Frozen(Matching.ImplementedInterfaces)]
            FakeCosmos<Record> sut,
            TestCosmosService<Record> service)
        {
            service.Reader.Should().BeSameAs(sut);
        }

        [Theory, AutoNSubstituteData]
        public void Should_Be_Able_To_Inject_As_Frozen_CosmosBulkReader(
            [Frozen(Matching.ImplementedInterfaces)]
            FakeCosmos<Record> sut,
            TestCosmosService<Record> service)
        {
            service.BulkReader.Should().BeSameAs(sut);
        }

        [Theory, AutoNSubstituteData]
        public void Should_Be_Able_To_Inject_As_Frozen_CosmosWriter(
            [Frozen(Matching.ImplementedInterfaces)]
            FakeCosmos<Record> sut,
            TestCosmosService<Record> service)
        {
            service.Writer.Should().BeSameAs(sut);
        }

        [Theory, AutoNSubstituteData]
        public void Should_Be_Able_To_Inject_As_Frozen_CosmosBulkWriter(
            [Frozen(Matching.ImplementedInterfaces)]
            FakeCosmos<Record> sut,
            TestCosmosService<Record> service)
        {
            service.BulkWriter.Should().BeSameAs(sut);
        }

        [Theory, AutoNSubstituteData]
        public void Should_Forward_CosmosReader_Calls(
            [Frozen, Substitute] FakeCosmosReader<Record> reader,
            [Greedy] FakeCosmos<Record> sut,
            string documentId,
            string partitionKey,
            QueryDefinition query)
        {
            var sutReader = (ICosmosReader<Record>)sut;
            _ = sutReader.FindAsync(documentId, partitionKey);
            _ = sutReader.ReadAsync(documentId, partitionKey);
            sutReader.ReadAllAsync(partitionKey);
            sutReader.QueryAsync(query, partitionKey);
            sutReader.QueryAsync<RecordAggregate>(query, partitionKey);

            _ = reader.Received(1).FindAsync(documentId, partitionKey);
            _ = reader.Received(1).ReadAsync(documentId, partitionKey);
            reader.Received(1).ReadAllAsync(partitionKey);
            reader.Received(1).QueryAsync(query, partitionKey);
            reader.Received(1).QueryAsync<RecordAggregate>(query, partitionKey);
        }

        [Theory, AutoNSubstituteData]
        public void Should_Forward_CosmosWriter_Calls(
            [Frozen, Substitute] FakeCosmosWriter<Record> writer,
            [Greedy] FakeCosmos<Record> sut,
            Record document,
            string documentId,
            string partitionKey,
            Action<Record> updateDocument,
            Func<Record, Task> updateDocumentAsync,
            Func<Record> getDefaultDocument)
        {
            var sutWriter = (ICosmosWriter<Record>)sut;
            _ = sutWriter.CreateAsync(document);
            _ = sutWriter.WriteAsync(document);
            _ = sutWriter.ReplaceAsync(document);
            _ = sutWriter.DeleteAsync(documentId, partitionKey);
            _ = sutWriter.UpdateAsync(documentId, partitionKey, updateDocument);
            _ = sutWriter.UpdateAsync(documentId, partitionKey, updateDocumentAsync);
            _ = sutWriter.UpdateOrCreateAsync(getDefaultDocument, updateDocument);
            _ = sutWriter.UpdateOrCreateAsync(getDefaultDocument, updateDocumentAsync);

            _ = writer.Received(1).CreateAsync(document);
            _ = writer.Received(1).WriteAsync(document);
            _ = writer.Received(1).ReplaceAsync(document);
            _ = writer.Received(1).DeleteAsync(documentId, partitionKey);
            _ = writer.Received(1).UpdateAsync(documentId, partitionKey, updateDocument);
            _ = writer.Received(1).UpdateAsync(documentId, partitionKey, updateDocumentAsync);
            _ = writer.Received(1).UpdateOrCreateAsync(getDefaultDocument, updateDocument);
            _ = writer.Received(1).UpdateOrCreateAsync(getDefaultDocument, updateDocumentAsync);
        }

        [Theory, AutoNSubstituteData]
        public void Should_Forward_CosmosBulkReader_Calls(
            [Frozen, Substitute] FakeCosmosReader<Record> reader,
            [Greedy] FakeCosmos<Record> sut,
            string documentId,
            string partitionKey,
            QueryDefinition query)
        {
            var sutReader = (ICosmosBulkReader<Record>)sut;
            _ = sutReader.FindAsync(documentId, partitionKey);
            _ = sutReader.ReadAsync(documentId, partitionKey);
            sutReader.ReadAllAsync(partitionKey);
            sutReader.QueryAsync(query, partitionKey);
            sutReader.QueryAsync<RecordAggregate>(query, partitionKey);

            _ = reader.Received(1).FindAsync(documentId, partitionKey);
            _ = reader.Received(1).ReadAsync(documentId, partitionKey);
            reader.Received(1).ReadAllAsync(partitionKey);
            reader.Received(1).QueryAsync(query, partitionKey);
            reader.Received(1).QueryAsync<RecordAggregate>(query, partitionKey);
        }

        [Theory, AutoNSubstituteData]
        public void Should_Forward_CosmosBulkWriter_Calls(
            [Frozen, Substitute] FakeCosmosWriter<Record> writer,
            [Greedy] FakeCosmos<Record> sut,
            Record document,
            string documentId,
            string partitionKey)
        {
            var sutWriter = (ICosmosBulkWriter<Record>)sut;
            _ = sutWriter.CreateAsync(document);
            _ = sutWriter.WriteAsync(document);
            _ = sutWriter.ReplaceAsync(document);
            _ = sutWriter.DeleteAsync(documentId, partitionKey);

            _ = writer.Received(1).CreateAsync(document);
            _ = writer.Received(1).WriteAsync(document);
            _ = writer.Received(1).ReplaceAsync(document);
            _ = writer.Received(1).DeleteAsync(documentId, partitionKey);
        }
    }
}