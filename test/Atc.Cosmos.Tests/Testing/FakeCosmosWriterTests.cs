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
    public class FakeCosmosWriterTests
    {
        [Theory, AutoNSubstituteData]
        public async Task CreateAsync_Should_Add_Document(
            FakeCosmosWriter<Record> sut,
            Record record)
        {
            var result = await sut.CreateAsync(record);
            result
                .Should()
                .BeEquivalentTo(
                    record,
                    o => o.Excluding(d => d.ETag));
            sut.Documents
                .Should()
                .ContainEquivalentOf(result);
        }

        [Theory, AutoNSubstituteData]
        public async Task CreateAsync_Should_Return_Document_With_ETag(
            FakeCosmosWriter<Record> sut,
            Record record)
        {
            record.ETag = null;
            var result = await sut.CreateAsync(record);
            result.ETag
                .Should()
                .NotBeNullOrEmpty();
        }

        [Theory, AutoNSubstituteData]
        public void CreateAsync_Should_Throw_If_Document_Exists(
            FakeCosmosWriter<Record> sut,
            Record record)
        {
            sut.Documents.Add(record);
            FluentActions.Awaiting(() => sut.CreateAsync(record))
                .Should()
                .Throw<CosmosException>()
                .Where(e => e.StatusCode == System.Net.HttpStatusCode.Conflict);
        }

        [Theory, AutoNSubstituteData]
        public async Task WriteAsync_Should_Add_Document(
            FakeCosmosWriter<Record> sut,
            Record record)
        {
            var result = await sut.WriteAsync(record);
            sut.Documents
                .Should()
                .ContainEquivalentOf(result);

            result
                .Should()
                .BeEquivalentTo(
                    record,
                    o => o.Excluding(d => d.ETag));
        }

        [Theory, AutoNSubstituteData]
        public async Task WriteAsync_Should_Return_Document_With_ETag(
            FakeCosmosWriter<Record> sut,
            Record record)
        {
            record.ETag = null;
            var result = await sut.WriteAsync(record);
            result.ETag
                .Should()
                .NotBeNullOrEmpty();
        }

        [Theory, AutoNSubstituteData]
        public async Task WriteAsync_Should_Replace_Document_If_Exists(
            FakeCosmosWriter<Record> sut,
            Record record)
        {
            var existingDocument = new Record
            {
                Id = record.Id,
                Pk = record.Pk,
            };
            sut.Documents.Add(existingDocument);

            var result = await sut.WriteAsync(record);
            result
                .Should()
                .BeEquivalentTo(
                    record,
                    o => o.Excluding(d => d.ETag));
            sut.Documents
                .Should()
                .NotContain(existingDocument)
                .And
                .ContainEquivalentOf(result);
        }

        [Theory, AutoNSubstituteData]
        public void ReplaceAsync_Should_Throw_If_Document_Does_Not_Exists(
            FakeCosmosWriter<Record> sut,
            Record record)
        {
            FluentActions.Awaiting(() => sut.ReplaceAsync(record))
                .Should()
                .Throw<CosmosException>()
                .Where(e => e.StatusCode == System.Net.HttpStatusCode.NotFound);
        }

        [Theory, AutoNSubstituteData]
        public async Task ReplaceAsync_Should_Replace_Existing_Document(
            FakeCosmosWriter<Record> sut,
            Record record)
        {
            var existingDocument = new Record
            {
                Id = record.Id,
                Pk = record.Pk,
                ETag = record.ETag,
            };
            sut.Documents.Add(existingDocument);

            var result = await sut.ReplaceAsync(record);
            result
                .Should()
                .BeEquivalentTo(
                    record,
                    o => o.Excluding(d => d.ETag));
            sut.Documents
                .Should()
                .NotContain(existingDocument)
                .And
                .ContainEquivalentOf(result);
        }

        [Theory, AutoNSubstituteData]
        public void ReplaceAsync_Should_Throw_If_Existing_Document_Has_Different_ETag(
           FakeCosmosWriter<Record> sut,
           Record record,
           string differentETag)
        {
            var existingDocument = new Record
            {
                Id = record.Id,
                Pk = record.Pk,
                ETag = differentETag,
            };
            sut.Documents.Add(existingDocument);

            FluentActions.Awaiting(() => sut.ReplaceAsync(record))
                .Should()
                .Throw<CosmosException>()
                .Where(e => e.StatusCode == System.Net.HttpStatusCode.PreconditionFailed);
        }

        [Theory, AutoNSubstituteData]
        public async Task ReplaceAsync_Should_Return_Document_With_ETag(
            FakeCosmosWriter<Record> sut,
            Record record)
        {
            sut.Documents.Add(new Record
            {
                Id = record.Id,
                Pk = record.Pk,
            });

            record.ETag = null;
            var result = await sut.ReplaceAsync(record);
            result.ETag
                .Should()
                .NotBeNullOrEmpty();
        }

        [Theory, AutoNSubstituteData]
        public void DeleteAsync_Should_Throw_If_Document_Does_Not_Exists(
            FakeCosmosWriter<Record> sut,
            string documentId,
            string partitionKey)
        {
            FluentActions.Awaiting(() => sut.DeleteAsync(documentId, partitionKey))
                .Should()
                .Throw<CosmosException>()
                .Where(e => e.StatusCode == System.Net.HttpStatusCode.NotFound);
        }

        [Theory, AutoNSubstituteData]
        public async Task DeleteAsync_Should_Replace_Existing_Document(
            FakeCosmosWriter<Record> sut,
            Record record)
        {
            var existingDocument = new Record
            {
                Id = record.Id,
                Pk = record.Pk,
            };
            sut.Documents.Add(existingDocument);

            await sut.DeleteAsync(record.Id, record.Pk);
            sut.Documents
                .Should()
                .NotContain(existingDocument);
        }

        [Theory, AutoNSubstituteData]
        public void UpdateAsync_Should_Throw_If_Document_Does_Not_Exists(
             FakeCosmosWriter<Record> sut,
             string documentId,
             string partitionKey)
        {
            FluentActions.Awaiting(() => sut
                .UpdateAsync(
                    documentId,
                    partitionKey,
                    d => { }))
                .Should()
                .Throw<CosmosException>()
                .Where(e => e.StatusCode == System.Net.HttpStatusCode.NotFound);
        }

        [Theory, AutoNSubstituteData]
        public async Task UpdateAsync_Should_Call_UpdateDocument_Delegate(
             FakeCosmosWriter<Record> sut,
             Record record,
             [Substitute] Action<Record> updateDocument)
        {
            sut.Documents.Add(record);

            var result = await sut.UpdateAsync(
                record.Id,
                record.Pk,
                updateDocument);

            result
                .Should()
                .BeEquivalentTo(
                    record,
                    o => o.Excluding(d => d.ETag));
            updateDocument
                .Received(1)
                .Invoke(result);
        }

        [Theory, AutoNSubstituteData]
        public async Task UpdateAsync_Should_Return_Updated_Document(
             FakeCosmosWriter<Record> sut,
             Record record,
             string newData)
        {
            record.ETag = null;
            sut.Documents.Add(record);

            var result = await sut.UpdateAsync(
                record.Id,
                record.Pk,
                d => d.Data = newData);

            result
                .Should()
                .BeEquivalentTo(
                    new Record
                    {
                        Id = record.Id,
                        Pk = record.Pk,
                        Data = newData,
                    },
                    o => o.Excluding(r => r.ETag));

            result.ETag
                .Should()
                .NotBeNullOrEmpty();
        }

        [Theory, AutoNSubstituteData]
        public async Task UpdateOrCreateAsync_Should_Call_GetDefaultDocument_Delegate(
             FakeCosmosWriter<Record> sut,
             Record defaultDocument,
             [Substitute] Func<Record> getDefaultDocument,
             [Substitute] Action<Record> updateDocument)
        {
            getDefaultDocument
                .Invoke()
                .Returns(defaultDocument);

            await sut.UpdateOrCreateAsync(getDefaultDocument, updateDocument);

            getDefaultDocument
                .Received(1)
                .Invoke();
        }

        [Theory, AutoNSubstituteData]
        public async Task UpdateOrCreateAsync_Should_Call_UpdateDocument_With_DefaultDocument(
             FakeCosmosWriter<Record> sut,
             Record defaultDocument,
             [Substitute] Action<Record> updateDocument)
        {
            var result = await sut.UpdateOrCreateAsync(
                () => defaultDocument,
                updateDocument);

            result
                .Should()
                .BeEquivalentTo(
                    defaultDocument,
                    o => o.Excluding(d => d.ETag));
            updateDocument
                .Received(1)
                .Invoke(result);
        }

        [Theory, AutoNSubstituteData]
        public async Task UpdateOrCreateAsync_Should_Add_NonExisting_Document(
             FakeCosmosWriter<Record> sut,
             Record defaultDocument,
             [Substitute] Action<Record> updateDocument)
        {
            var result = await sut.UpdateOrCreateAsync(
                () => defaultDocument,
                updateDocument);

            sut.Documents
                .Should()
                .ContainEquivalentOf(result);

            result
                .Should()
                .BeEquivalentTo(
                    defaultDocument,
                    o => o.Excluding(d => d.ETag));
        }

        [Theory, AutoNSubstituteData]
        public async Task UpdateOrCreateAsync_Should_Call_UpdateDocument_With_ExistingDocument(
             FakeCosmosWriter<Record> sut,
             Record existingDocument,
             [Substitute] Action<Record> updateDocument)
        {
            sut.Documents.Add(existingDocument);
            var defaultDocument = new Record
            {
                Id = existingDocument.Id,
                Pk = existingDocument.Pk,
            };

            var result = await sut.UpdateOrCreateAsync(
                () => defaultDocument,
                updateDocument);

            updateDocument
                .Received(1)
                .Invoke(result);

            result
                .Should()
                .BeEquivalentTo(
                    existingDocument,
                    o => o.Excluding(d => d.ETag));
        }

        [Theory, AutoNSubstituteData]
        public async Task UpdateOrCreateAsync_Should_Return_Updated_Document(
             FakeCosmosWriter<Record> sut,
             Record document,
             string newData)
        {
            document.ETag = null;
            sut.Documents.Add(document);
            var defaultDocument = new Record
            {
                Id = document.Id,
                Pk = document.Pk,
            };

            var result = await sut.UpdateOrCreateAsync(
                () => defaultDocument,
                d => d.Data = newData);

            result
                .Should()
                .BeEquivalentTo(
                    new Record
                    {
                        Id = document.Id,
                        Pk = document.Pk,
                        Data = newData,
                    },
                    o => o.Excluding(r => r.ETag));

            result.ETag
                .Should()
                .NotBeNullOrEmpty();
        }

        [Theory, AutoNSubstituteData]
        public void Should_Be_Able_To_Inject_As_Frozen_CosmosWriter(
            [Frozen(Matching.ImplementedInterfaces)]
            FakeCosmosWriter<Record> sut,
            TestCosmosService<Record> service)
        {
            service.Writer.Should().BeSameAs(sut);
        }

        [Theory, AutoNSubstituteData]
        public void Should_Be_Able_To_Inject_As_Frozen_CosmosBulkWriter(
            [Frozen(Matching.ImplementedInterfaces)]
            FakeCosmosWriter<Record> sut,
            TestCosmosService<Record> service)
        {
            service.BulkWriter.Should().BeSameAs(sut);
        }
    }
}