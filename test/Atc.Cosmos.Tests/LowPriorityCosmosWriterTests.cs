#if PREVIEW
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Atc.Cosmos.Internal;
using Atc.Cosmos.Serialization;
using Atc.Test;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using NSubstitute;
using Xunit;

namespace Atc.Cosmos.Tests
{
    public class LowPriorityCosmosWriterTests
    {
        private readonly Record record;
        private readonly Container container;
        private readonly ICosmosContainerProvider containerProvider;
        private readonly ILowPriorityCosmosReader<Record> reader;
        private readonly IJsonCosmosSerializer serializer;
        private readonly LowPriorityCosmosWriter<Record> sut;

        public LowPriorityCosmosWriterTests()
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
            container
                .PatchItemAsync<object>(default, default, default, default)
                .ReturnsForAnyArgs(response);

            reader = Substitute.For<ILowPriorityCosmosReader<Record>>();
            reader
                .ReadAsync(default, default, default)
                .ReturnsForAnyArgs(record);

            serializer = Substitute.For<IJsonCosmosSerializer>();
            serializer
                .FromString<Record>(default)
                .ReturnsForAnyArgs(new Fixture().Create<Record>());

            sut = new LowPriorityCosmosWriter<Record>(containerProvider, reader, serializer);
        }

        [Fact]
        public void Implements_Interface()
            => sut.Should().BeAssignableTo<ILowPriorityCosmosWriter<Record>>();

        [Theory, AutoNSubstituteData]
        public async Task WriteAsync_Uses_The_Right_Container(
            CancellationToken cancellationToken)
        {
            await sut.WriteAsync(record, cancellationToken);
            containerProvider
                .Received(1)
                .GetContainer<Record>(
                    allowBulk: false);
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
                    Arg.Is<ItemRequestOptions>(c => c.PriorityLevel == PriorityLevel.Low),
                    cancellationToken);
        }

        [Theory, AutoNSubstituteData]
        public async Task WriteWithNoResponseAsync_UpsertItem_In_Container(
            CancellationToken cancellationToken)
        {
            containerProvider
                .GetContainer<Record>()
                .ReturnsForAnyArgs(container);

            await sut.WriteWithNoResponseAsync(record, cancellationToken);
            await container
                .Received(1)
                .UpsertItemAsync<object>(
                    record,
                    new PartitionKey(record.Pk),
                    Arg.Is<ItemRequestOptions>(
                        p => p.EnableContentResponseOnWrite == false && p.PriorityLevel == PriorityLevel.Low),
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
                    Arg.Is<ItemRequestOptions>(o => o.PriorityLevel == PriorityLevel.Low),
                    cancellationToken);
        }

        [Theory, AutoNSubstituteData]
        public async Task CreateWithNoResponseAsync_Calls_CreateItem_On_Container(
           CancellationToken cancellationToken)
        {
            await sut.CreateWithNoResponseAsync(record, cancellationToken);
            _ = container
                .Received(1)
                .CreateItemAsync<object>(
                    record,
                    new PartitionKey(record.Pk),
                    Arg.Is<ItemRequestOptions>(p => p.EnableContentResponseOnWrite == false && p.PriorityLevel == PriorityLevel.Low),
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
                    Arg.Is<ItemRequestOptions>(
                        o => o.IfMatchEtag == record.ETag && o.PriorityLevel == PriorityLevel.Low),
                    cancellationToken);
        }

        [Theory, AutoNSubstituteData]
        public async Task ReplaceWithNoResponseAsync_Calls_ReplaceItemAsync_On_Container(
           CancellationToken cancellationToken)
        {
            await sut.ReplaceWithNoResponseAsync(record, cancellationToken);
            _ = container
                .Received(1)
                .ReplaceItemAsync<object>(
                    record,
                    record.Id,
                    new PartitionKey(record.Pk),
                    Arg.Is<ItemRequestOptions>(o => o.IfMatchEtag == record.ETag
                                                 && o.EnableContentResponseOnWrite == false
                                                 && o.PriorityLevel == PriorityLevel.Low),
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
                    Arg.Is<ItemRequestOptions>(o => o.PriorityLevel == PriorityLevel.Low),
                    cancellationToken: cancellationToken);
        }

        [Theory, AutoNSubstituteData]
        public async Task Should_Return_True_When_Trying_To_Delete_Existing_Resource(
           CancellationToken cancellationToken)
        {
            var deleted = await sut.TryDeleteAsync(
                record.Id,
                record.Pk,
                cancellationToken);

            deleted
                .Should()
                .BeTrue();

            _ = container
                .Received(1)
                .DeleteItemAsync<object>(
                    record.Id,
                    new PartitionKey(record.Pk),
                    Arg.Is<ItemRequestOptions>(o => o.PriorityLevel == PriorityLevel.Low),
                    cancellationToken: cancellationToken);
        }

        [Theory, AutoNSubstituteData]
        public async Task Should_Return_False_When_Trying_To_Delete_NonExisting_Resource(
           CancellationToken cancellationToken)
        {
            container
                .DeleteItemAsync<object>(default, default, default, default)
                .ReturnsForAnyArgs<ItemResponse<object>>(
                    r => throw new CosmosException("fake", HttpStatusCode.NotFound, 0, "1", 1));

            var deleted = await sut.TryDeleteAsync(
                record.Id,
                record.Pk,
                cancellationToken);

            deleted
                .Should()
                .BeFalse();

            _ = container
                .Received(1)
                .DeleteItemAsync<object>(
                    record.Id,
                    new PartitionKey(record.Pk),
                    Arg.Is<ItemRequestOptions>(o => o.PriorityLevel == PriorityLevel.Low),
                    cancellationToken: cancellationToken);
        }

        [Theory, AutoNSubstituteData]
        public async Task UpdateAsync_Reads_The_Resource(
            string documentId,
            string partitionKey,
            Action<Record> updateDocument,
            int retries,
            CancellationToken cancellationToken)
        {
            await sut.UpdateAsync(
                documentId,
                partitionKey,
                updateDocument,
                retries,
                cancellationToken);

            _ = reader
                .Received(1)
                .ReadAsync(
                    documentId,
                    partitionKey,
                    cancellationToken);
        }

        [Theory, AutoNSubstituteData]
        public async Task UpdateAsync_Calls_UpdateDocument_With_Read_Resource(
            string documentId,
            string partitionKey,
            [Substitute] Action<Record> updateDocument,
            int retries,
            CancellationToken cancellationToken)
        {
            await sut.UpdateAsync(
                documentId,
                partitionKey,
                updateDocument,
                retries,
                cancellationToken);

            updateDocument
                .Received(1)
                .Invoke(record);
        }

        [Theory, AutoNSubstituteData]
        public async Task UpdateAsync_Calls_ReplaceItem_With_Updated_Resource(
            string documentId,
            string partitionKey,
            [Substitute] Action<Record> updateDocument,
            int retries,
            CancellationToken cancellationToken)
        {
            await sut.UpdateAsync(
                documentId,
                partitionKey,
                updateDocument,
                retries,
                cancellationToken);

            _ = container
                .Received(1)
                .ReplaceItemAsync<object>(
                    record,
                    record.Id,
                    new PartitionKey(record.Pk),
                    Arg.Is<ItemRequestOptions>(o => o.IfMatchEtag == record.ETag && o.PriorityLevel == PriorityLevel.Low),
                    cancellationToken);
        }

        [Theory, AutoNSubstituteData]
        public async Task UpdateOrCreateAsync_Finds_The_Resource(
           Action<Record> updateDocument,
           int retries,
           Record defaultDocument,
           CancellationToken cancellationToken)
        {
            await sut.UpdateOrCreateAsync(
                () => defaultDocument,
                updateDocument,
                retries,
                cancellationToken);

            _ = reader
                .Received(1)
                .FindAsync(
                    defaultDocument.Id,
                    defaultDocument.Pk,
                    cancellationToken);
        }

        [Theory, AutoNSubstituteData]
        public async Task UpdateAsync_Calls_UpdateDocument_With_Found_Resource(
            [Substitute] Action<Record> updateDocument,
            int retries,
            Record defaultDocument,
            Record foundResource,
            CancellationToken cancellationToken)
        {
            reader
                .FindAsync(default, default, default)
                .ReturnsForAnyArgs(foundResource);

            await sut.UpdateOrCreateAsync(
                () => defaultDocument,
                updateDocument,
                retries,
                cancellationToken);

            updateDocument
                .Received(1)
                .Invoke(foundResource);
        }

        [Theory, AutoNSubstituteData]
        public async Task UpdateAsync_Calls_UpdateDocument_With_Default_Document_If_Not_Found(
            [Substitute] Action<Record> updateDocument,
            int retries,
            Record defaultDocument,
            CancellationToken cancellationToken)
        {
            await sut.UpdateOrCreateAsync(
                () => defaultDocument,
                updateDocument,
                retries,
                cancellationToken);

            updateDocument
                .Received(1)
                .Invoke(defaultDocument);
        }

        [Theory, AutoNSubstituteData]
        public async Task UpdateOrCreateAsync_Calls_ReplaceItem_If_Resource_Has_ETag(
            [Substitute] Action<Record> updateDocument,
            int retries,
            Record defaultDocument,
            Record foundResource,
            string etag,
            CancellationToken cancellationToken)
        {
            ((ICosmosResource)foundResource).ETag = etag;
            reader
                .FindAsync(default, default, default)
                .ReturnsForAnyArgs(foundResource);

            await sut.UpdateOrCreateAsync(
                () => defaultDocument,
                updateDocument,
                retries,
                cancellationToken);

            _ = container
                .Received(1)
                .ReplaceItemAsync<object>(
                    foundResource,
                    foundResource.Id,
                    new PartitionKey(foundResource.Pk),
                    Arg.Is<ItemRequestOptions>(o => o.IfMatchEtag == foundResource.ETag && o.PriorityLevel == PriorityLevel.Low),
                    cancellationToken);
        }

        [Theory, AutoNSubstituteData]
        public async Task UpdateOrCreateAsync_Calls_CreateItem_If_Resource_Has_No_ETag(
            [Substitute] Action<Record> updateDocument,
            int retries,
            Record defaultDocument,
            CancellationToken cancellationToken)
        {
            defaultDocument.ETag = null;
            await sut.UpdateOrCreateAsync(
                () => defaultDocument,
                updateDocument,
                retries,
                cancellationToken);

            _ = container
                .Received(1)
                .CreateItemAsync<object>(
                    defaultDocument,
                    new PartitionKey(defaultDocument.Pk),
                    Arg.Is<ItemRequestOptions>(o => o.PriorityLevel == PriorityLevel.Low),
                    cancellationToken);
        }

        [Theory, AutoNSubstituteData]
        public async Task PatchAsync_Calls_PatchItemAsync_On_Container(
            IReadOnlyList<PatchOperation> patchOperations,
            string filterPredicate,
            CancellationToken cancellationToken)
        {
            await sut.PatchAsync(
                record.Id,
                record.Pk,
                patchOperations,
                filterPredicate,
                cancellationToken);

            _ = container
                .Received(1)
                .PatchItemAsync<object>(
                    record.Id,
                    new PartitionKey(record.Pk),
                    patchOperations,
                    Arg.Is<PatchItemRequestOptions>(o => o.PriorityLevel == PriorityLevel.Low),
                    cancellationToken);
        }

        [Theory, AutoNSubstituteData]
        public async Task PatchWithNoResponseAsync_Calls_PatchItemAsync_On_Container(
            IReadOnlyList<PatchOperation> patchOperations,
            string filterPredicate,
            CancellationToken cancellationToken)
        {
            await sut.PatchWithNoResponseAsync(
                record.Id,
                record.Pk,
                patchOperations,
                filterPredicate,
                cancellationToken);

            _ = container
                .Received(1)
                .PatchItemAsync<object>(
                    record.Id,
                    new PartitionKey(record.Pk),
                    patchOperations,
                    Arg.Is<PatchItemRequestOptions>(o => o.PriorityLevel == PriorityLevel.Low),
                    cancellationToken);
        }
    }
}
#endif