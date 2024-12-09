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
    public class CosmosWriterTests
    {
        private readonly Record record;
        private readonly Container container;
        private readonly ICosmosContainerProvider containerProvider;
        private readonly ICosmosReader<Record> reader;
        private readonly IJsonCosmosSerializer serializer;
        private readonly CosmosWriter<Record> sut;

        public CosmosWriterTests()
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
#if PREVIEW

            var responseMessage = Substitute.For<ResponseMessage>();
            responseMessage.StatusCode.Returns(HttpStatusCode.Accepted);
            container
                .DeleteAllItemsByPartitionKeyStreamAsync(default, default, default)
                .ReturnsForAnyArgs(responseMessage);
#endif

            reader = Substitute.For<ICosmosReader<Record>>();
            reader
                .ReadAsync(default, default, default)
                .ReturnsForAnyArgs(record);

            serializer = Substitute.For<IJsonCosmosSerializer>();
            serializer
                .FromString<Record>(default)
                .ReturnsForAnyArgs(new Fixture().Create<Record>());

            sut = new CosmosWriter<Record>(containerProvider, reader, serializer);
        }

        [Fact]
        public void Implements_Interface()
            => sut.Should().BeAssignableTo<ICosmosWriter<Record>>();

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
#if PREVIEW
                    Arg.Is<ItemRequestOptions>(o => o.PriorityLevel == PriorityLevel.High),
#else
                    Arg.Any<ItemRequestOptions>(),
#endif
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
#if PREVIEW
                    Arg.Is<ItemRequestOptions>(p => p.EnableContentResponseOnWrite == false && p.PriorityLevel == PriorityLevel.High),
#else
                    Arg.Is<ItemRequestOptions>(p => p.EnableContentResponseOnWrite == false),
#endif
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
#if PREVIEW
                    Arg.Is<ItemRequestOptions>(o => o.PriorityLevel == PriorityLevel.High),
#else
                    Arg.Any<ItemRequestOptions>(),
#endif
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
#if PREVIEW
                    Arg.Is<ItemRequestOptions>(p => p.EnableContentResponseOnWrite == false && p.PriorityLevel == PriorityLevel.High),
#else
                    Arg.Is<ItemRequestOptions>(p => p.EnableContentResponseOnWrite == false),
#endif
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
#if PREVIEW
                    Arg.Is<ItemRequestOptions>(o => o.IfMatchEtag == record.ETag && o.PriorityLevel == PriorityLevel.High),
#else
                    Arg.Is<ItemRequestOptions>(o => o.IfMatchEtag == record.ETag),
#endif
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
                    Arg.Is<ItemRequestOptions>(
                        o => o.IfMatchEtag == record.ETag
#if PREVIEW
                             && o.PriorityLevel == PriorityLevel.High
#endif
                             && o.EnableContentResponseOnWrite == false),
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
#if PREVIEW
                    Arg.Is<ItemRequestOptions>(o => o.PriorityLevel == PriorityLevel.High),
#else
                    Arg.Any<ItemRequestOptions>(),
#endif
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
#if PREVIEW
                    Arg.Is<ItemRequestOptions>(o => o.PriorityLevel == PriorityLevel.High),
#else
                    Arg.Any<ItemRequestOptions>(),
#endif
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
#if PREVIEW
                    Arg.Is<ItemRequestOptions>(o => o.PriorityLevel == PriorityLevel.High),
#else
                    Arg.Any<ItemRequestOptions>(),
#endif
                    cancellationToken: cancellationToken);
        }

        [Theory, AutoNSubstituteData]
        public async Task DeletePartitionAsync_Calls_DeleteAllItemsByPartitionKeyStreamAsync_On_Container(
            CancellationToken cancellationToken)
        {
            await sut.DeletePartitionAsync(record.Pk, cancellationToken);
            _ = container
                .Received(1)
                .DeleteAllItemsByPartitionKeyStreamAsync(
                    new PartitionKey(record.Pk),
                    Arg.Is<ItemRequestOptions>(o => o.PriorityLevel == PriorityLevel.High),
                    cancellationToken: cancellationToken);
        }

        [Theory, AutoNSubstituteData]
        public Task DeletePartitionAsync_Throws_CosmosException_If_ResponseMessage_Is_Not_Sucessful(
            CancellationToken cancellationToken)
        {
            using var responseMessage = new ResponseMessage(HttpStatusCode.BadRequest);
            container
                .DeleteAllItemsByPartitionKeyStreamAsync(default, default, default)
                .ReturnsForAnyArgs(responseMessage);

            Func<Task> act = () => sut.DeletePartitionAsync(record.Pk, cancellationToken);
            return act.Should().ThrowAsync<CosmosException>();
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
#if PREVIEW
                    Arg.Is<ItemRequestOptions>(
                        o => o.IfMatchEtag == record.ETag && o.PriorityLevel == PriorityLevel.High),
#else
                    Arg.Is<ItemRequestOptions>(o => o.IfMatchEtag == record.ETag),
#endif
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
                    Arg.Is<ItemRequestOptions>(o => o.IfMatchEtag == ((ICosmosResource)foundResource).ETag),
                    cancellationToken);
        }

        [Theory, AutoNSubstituteData]
        public async Task UpdateOrCreateAsync_Calls_CreateItem_If_Resource_Has_No_ETag(
            [Substitute] Action<Record> updateDocument,
            int retries,
            Record defaultDocument,
            CancellationToken cancellationToken)
        {
            ((ICosmosResource)defaultDocument).ETag = null;
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
#if PREVIEW
                    Arg.Is<ItemRequestOptions>(o => o.PriorityLevel == PriorityLevel.High),
#else
                    Arg.Any<ItemRequestOptions>(),
#endif
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
#if PREVIEW
                    Arg.Is<PatchItemRequestOptions>(o => o.PriorityLevel == PriorityLevel.High),
#else
                    Arg.Any<PatchItemRequestOptions>(),
#endif
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
#if PREVIEW
                    Arg.Is<PatchItemRequestOptions>(o => o.PriorityLevel == PriorityLevel.High),
#else
                    Arg.Any<PatchItemRequestOptions>(),
#endif
                    cancellationToken);
        }
    }
}