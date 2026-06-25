namespace Atc.Cosmos.Tests;

public sealed class CosmosWriterTests
{
    private readonly Record record;
    private readonly Container container;
    private readonly ICosmosContainerProvider containerProvider;
    private readonly ICosmosReader<Record> reader;
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
            .CreateItemAsync<object>(item: null, partitionKey: null, requestOptions: null, CancellationToken.None)
            .ReturnsForAnyArgs(response);

        container
            .ReplaceItemAsync<object>(item: null, id: null, partitionKey: null, requestOptions: null, CancellationToken.None)
            .ReturnsForAnyArgs(response);

        container
            .UpsertItemAsync<object>(item: null, partitionKey: null, requestOptions: null, CancellationToken.None)
            .ReturnsForAnyArgs(response);

        container
            .PatchItemAsync<object>(id: null, partitionKey: default, patchOperations: null, requestOptions: null)
            .ReturnsForAnyArgs(response);

        var responseMessage = Substitute.For<ResponseMessage>();
        responseMessage.StatusCode.Returns(HttpStatusCode.Accepted);

        container
            .DeleteAllItemsByPartitionKeyStreamAsync(partitionKey: default, requestOptions: null, CancellationToken.None)
            .ReturnsForAnyArgs(responseMessage);

        reader = Substitute.For<ICosmosReader<Record>>();

        reader
            .ReadAsync(documentId: null, partitionKey: null, CancellationToken.None)
            .ReturnsForAnyArgs(record);

        var serializer = Substitute.For<IJsonCosmosSerializer>();

        serializer
            .FromString<Record>(Arg.Any<string>())
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
        // Act
        await sut.WriteAsync(record, cancellationToken);

        // Assert
        containerProvider
            .Received(1)
            .GetContainer<Record>(
                allowBulk: false);
    }

    [Theory, AutoNSubstituteData]
    public async Task WriteAsync_UpsertItem_In_Container(
        CancellationToken cancellationToken)
    {
        // Arrange
        containerProvider
            .GetContainer<Record>()
            .ReturnsForAnyArgs(container);

        // Act
        await sut.WriteAsync(record, cancellationToken);

        // Assert
        await container
            .Received(1)
            .UpsertItemAsync<object>(
                record,
                new PartitionKey(record.Pk),
                Arg.Is<ItemRequestOptions>(o => o.PriorityLevel == PriorityLevel.High),
                cancellationToken);
    }

    [Theory, AutoNSubstituteData]
    public async Task WriteWithNoResponseAsync_UpsertItem_In_Container(
        CancellationToken cancellationToken)
    {
        // Arrange
        containerProvider
            .GetContainer<Record>()
            .ReturnsForAnyArgs(container);

        // Act
        await sut.WriteWithNoResponseAsync(record, cancellationToken);

        // Assert
        await container
            .Received(1)
            .UpsertItemAsync<object>(
                record,
                new PartitionKey(record.Pk),
                Arg.Is<ItemRequestOptions>(p => p.EnableContentResponseOnWrite == false && p.PriorityLevel == PriorityLevel.High),
                cancellationToken);
    }

    [Theory, AutoNSubstituteData]
    public async Task CreateAsync_Calls_CreateItem_On_Container(
        CancellationToken cancellationToken)
    {
        // Act
        await sut.CreateAsync(record, cancellationToken);

        // Assert
        _ = container
            .Received(1)
            .CreateItemAsync<object>(
                record,
                new PartitionKey(record.Pk),
                Arg.Is<ItemRequestOptions>(o => o.PriorityLevel == PriorityLevel.High),
                cancellationToken);
    }

    [Theory, AutoNSubstituteData]
    public async Task CreateWithNoResponseAsync_Calls_CreateItem_On_Container(
        CancellationToken cancellationToken)
    {
        // Act
        await sut.CreateWithNoResponseAsync(record, cancellationToken);

        // Assert
        _ = container
            .Received(1)
            .CreateItemAsync<object>(
                record,
                new PartitionKey(record.Pk),
                Arg.Is<ItemRequestOptions>(p => p.EnableContentResponseOnWrite == false && p.PriorityLevel == PriorityLevel.High),
                cancellationToken);
    }

    [Theory, AutoNSubstituteData]
    public async Task ReplaceAsync_Calls_ReplaceItemAsync_On_Container(
        CancellationToken cancellationToken)
    {
        // Act
        await sut.ReplaceAsync(record, cancellationToken);

        // Assert
        _ = container
            .Received(1)
            .ReplaceItemAsync<object>(
                record,
                record.Id,
                new PartitionKey(record.Pk),
                Arg.Is<ItemRequestOptions>(o => o.IfMatchEtag == record.ETag && o.PriorityLevel == PriorityLevel.High),
                cancellationToken);
    }

    [Theory, AutoNSubstituteData]
    public async Task ReplaceWithNoResponseAsync_Calls_ReplaceItemAsync_On_Container(
        CancellationToken cancellationToken)
    {
        // Act
        await sut.ReplaceWithNoResponseAsync(record, cancellationToken);

        // Assert
        _ = container
            .Received(1)
            .ReplaceItemAsync<object>(
                record,
                record.Id,
                new PartitionKey(record.Pk),
                Arg.Is<ItemRequestOptions>(
                    o => o.IfMatchEtag == record.ETag
                         && o.PriorityLevel == PriorityLevel.High
                         && o.EnableContentResponseOnWrite == false),
                cancellationToken);
    }

    [Theory, AutoNSubstituteData]
    public void Multiple_Operations_Uses_Same_Container(
        CancellationToken cancellationToken)
    {
        // Act
        _ = sut.WriteAsync(record, cancellationToken);
        _ = sut.WriteAsync(record, cancellationToken);
        _ = sut.CreateAsync(record, cancellationToken);
        _ = sut.CreateAsync(record, cancellationToken);
        _ = sut.ReplaceAsync(record, cancellationToken);
        _ = sut.ReplaceAsync(record, cancellationToken);

        // Assert
        container.ReceivedCalls().Should().HaveCount(6);
    }

    [Theory, AutoNSubstituteData]
    public async Task DeleteAsync_Calls_DeleteItemAsync_On_Container(
        CancellationToken cancellationToken)
    {
        // Act
        await sut.DeleteAsync(record.Id, record.Pk, cancellationToken);

        // Assert
        _ = container
            .Received(1)
            .DeleteItemAsync<object>(
                record.Id,
                new PartitionKey(record.Pk),
                Arg.Is<ItemRequestOptions>(o => o.PriorityLevel == PriorityLevel.High),
                cancellationToken: cancellationToken);
    }

    [Theory, AutoNSubstituteData]
    public async Task Should_Return_True_When_Trying_To_Delete_Existing_Resource(
        CancellationToken cancellationToken)
    {
        // Act
        var deleted = await sut.TryDeleteAsync(
            record.Id,
            record.Pk,
            cancellationToken);

        // Assert
        deleted.Should().BeTrue();

        _ = container
            .Received(1)
            .DeleteItemAsync<object>(
                record.Id,
                new PartitionKey(record.Pk),
                Arg.Is<ItemRequestOptions>(o => o.PriorityLevel == PriorityLevel.High),
                cancellationToken: cancellationToken);
    }

    [Theory, AutoNSubstituteData]
    public async Task Should_Return_False_When_Trying_To_Delete_NonExisting_Resource(
        CancellationToken cancellationToken)
    {
        // Arrange
        container
            .DeleteItemAsync<object>(id: null, partitionKey: default, requestOptions: null, CancellationToken.None)
            .ReturnsForAnyArgs<ItemResponse<object>>(
                r => throw new CosmosException("fake", HttpStatusCode.NotFound, 0, "1", 1));

        // Act
        var deleted = await sut.TryDeleteAsync(
            record.Id,
            record.Pk,
            cancellationToken);

        // Assert
        deleted.Should().BeFalse();

        _ = container
            .Received(1)
            .DeleteItemAsync<object>(
                record.Id,
                new PartitionKey(record.Pk),
                Arg.Is<ItemRequestOptions>(o => o.PriorityLevel == PriorityLevel.High),
                cancellationToken: cancellationToken);
    }

    [Theory, AutoNSubstituteData]
    public async Task DeletePartitionAsync_Calls_DeleteAllItemsByPartitionKeyStreamAsync_On_Container(
        CancellationToken cancellationToken)
    {
        // Act
        await sut.DeletePartitionAsync(record.Pk, cancellationToken);

        // Assert
        _ = container
            .Received(1)
            .DeleteAllItemsByPartitionKeyStreamAsync(
                new PartitionKey(record.Pk),
                Arg.Is<ItemRequestOptions>(o => o.PriorityLevel == PriorityLevel.High),
                cancellationToken: cancellationToken);
    }

    [Theory, AutoNSubstituteData]
    public async Task DeletePartitionAsync_Throws_CosmosException_If_ResponseMessage_Is_Not_Successful(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var responseMessage = new ResponseMessage(HttpStatusCode.BadRequest);

        container
            .DeleteAllItemsByPartitionKeyStreamAsync(default, requestOptions: null, CancellationToken.None)
            .ReturnsForAnyArgs(responseMessage);

        // Act & assert
        Func<Task> act = () => sut.DeletePartitionAsync(record.Pk, cancellationToken);
        await act.Should().ThrowAsync<CosmosException>();
    }

    [Theory, AutoNSubstituteData]
    public async Task UpdateAsync_Reads_The_Resource(
        string documentId,
        string partitionKey,
        Action<Record> updateDocument,
        int retries,
        CancellationToken cancellationToken)
    {
        // Act
        await sut.UpdateAsync(
            documentId,
            partitionKey,
            updateDocument,
            retries,
            cancellationToken);

        // Assert
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
        // Act
        await sut.UpdateAsync(
            documentId,
            partitionKey,
            updateDocument,
            retries,
            cancellationToken);

        // Assert
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
        // Act
        await sut.UpdateAsync(
            documentId,
            partitionKey,
            updateDocument,
            retries,
            cancellationToken);

        // Assert
        _ = container
            .Received(1)
            .ReplaceItemAsync<object>(
                record,
                record.Id,
                new PartitionKey(record.Pk),
                Arg.Is<ItemRequestOptions>(
                    o => o.IfMatchEtag == record.ETag && o.PriorityLevel == PriorityLevel.High),
                cancellationToken);
    }

    [Theory, AutoNSubstituteData]
    public async Task UpdateOrCreateAsync_Finds_The_Resource(
        Action<Record> updateDocument,
        int retries,
        Record defaultDocument,
        CancellationToken cancellationToken)
    {
        // Act
        await sut.UpdateOrCreateAsync(
            () => defaultDocument,
            updateDocument,
            retries,
            cancellationToken);

        // Assert
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
        // Arrange
        reader
            .FindAsync(documentId: null, partitionKey: null, CancellationToken.None)
            .ReturnsForAnyArgs(foundResource);

        // Act
        await sut.UpdateOrCreateAsync(
            () => defaultDocument,
            updateDocument,
            retries,
            cancellationToken);

        // Assert
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
        // Act
        await sut.UpdateOrCreateAsync(
            () => defaultDocument,
            updateDocument,
            retries,
            cancellationToken);

        // Assert
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
        // Arrange
        foundResource.ETag = etag;

        reader
            .FindAsync(documentId: null, partitionKey: null, CancellationToken.None)
            .ReturnsForAnyArgs(foundResource);

        // Act
        await sut.UpdateOrCreateAsync(
            () => defaultDocument,
            updateDocument,
            retries,
            cancellationToken);

        // Assert
        _ = container
            .Received(1)
            .ReplaceItemAsync<object>(
                foundResource,
                foundResource.Id,
                new PartitionKey(foundResource.Pk),
                Arg.Is<ItemRequestOptions>(o => o.IfMatchEtag == foundResource.ETag),
                cancellationToken);
    }

    [Theory, AutoNSubstituteData]
    public async Task UpdateOrCreateAsync_Calls_CreateItem_If_Resource_Has_No_ETag(
        [Substitute] Action<Record> updateDocument,
        int retries,
        Record defaultDocument,
        CancellationToken cancellationToken)
    {
        // Arrange
        defaultDocument.ETag = null;

        // Act
        await sut.UpdateOrCreateAsync(
            () => defaultDocument,
            updateDocument,
            retries,
            cancellationToken);

        // Assert
        _ = container
            .Received(1)
            .CreateItemAsync<object>(
                defaultDocument,
                new PartitionKey(defaultDocument.Pk),
                Arg.Is<ItemRequestOptions>(o => o.PriorityLevel == PriorityLevel.High),
                cancellationToken);
    }

    [Theory, AutoNSubstituteData]
    public async Task PatchAsync_Calls_PatchItemAsync_On_Container(
        IReadOnlyList<PatchOperation> patchOperations,
        string filterPredicate,
        CancellationToken cancellationToken)
    {
        // Act
        await sut.PatchAsync(
            record.Id,
            record.Pk,
            patchOperations,
            filterPredicate,
            cancellationToken);

        // Assert
        _ = container
            .Received(1)
            .PatchItemAsync<object>(
                record.Id,
                new PartitionKey(record.Pk),
                patchOperations,
                Arg.Is<PatchItemRequestOptions>(o => o.PriorityLevel == PriorityLevel.High),
                cancellationToken);
    }

    [Theory, AutoNSubstituteData]
    public async Task PatchWithNoResponseAsync_Calls_PatchItemAsync_On_Container(
        IReadOnlyList<PatchOperation> patchOperations,
        string filterPredicate,
        CancellationToken cancellationToken)
    {
        // Act
        await sut.PatchWithNoResponseAsync(
            record.Id,
            record.Pk,
            patchOperations,
            filterPredicate,
            cancellationToken);

        // Assert
        _ = container
            .Received(1)
            .PatchItemAsync<object>(
                record.Id,
                new PartitionKey(record.Pk),
                patchOperations,
                Arg.Is<PatchItemRequestOptions>(o => o.PriorityLevel == PriorityLevel.High),
                cancellationToken);
    }

    [Theory, AutoNSubstituteData]
    public async Task UpdateAsync_Retries_On_PreconditionFailed_And_Rethrows_When_Retries_Exhausted(
        string documentId,
        string partitionKey,
        CancellationToken cancellationToken)
    {
        // Arrange
        container
            .ReplaceItemAsync<object>(item: null, id: null, partitionKey: null, requestOptions: null, CancellationToken.None)
            .ReturnsForAnyArgs(Task.FromException<ItemResponse<object>>(CosmosError(HttpStatusCode.PreconditionFailed)));

        var act = () => sut.UpdateAsync(
            documentId,
            partitionKey,
            updateDocument: _ => { },
            retries: 2,
            cancellationToken);

        // Act & assert
        await act.Should().ThrowAsync<CosmosException>();

        _ = container
            .ReceivedWithAnyArgs(2)
            .ReplaceItemAsync<object>(item: null, id: null, partitionKey: null, requestOptions: null, CancellationToken.None);
    }

    [Theory, AutoNSubstituteData]
    public async Task UpdateAsync_Retries_On_PreconditionFailed_Then_Succeeds(
        string documentId,
        string partitionKey,
        CancellationToken cancellationToken)
    {
        // Arrange
        var response = Substitute.For<ItemResponse<object>>();
        response.Resource.Returns(new Fixture().Create<string>());

        container
            .ReplaceItemAsync<object>(item: null, id: null, partitionKey: null, requestOptions: null, CancellationToken.None)
            .ReturnsForAnyArgs(
                Task.FromException<ItemResponse<object>>(CosmosError(HttpStatusCode.PreconditionFailed)),
                Task.FromResult(response));

        var act = () => sut.UpdateAsync(
            documentId,
            partitionKey,
            updateDocument: _ => { },
            retries: 2,
            cancellationToken);

        // Act & assert
        await act.Should().NotThrowAsync();

        _ = container
            .ReceivedWithAnyArgs(2)
            .ReplaceItemAsync<object>(item: null, id: null, partitionKey: null, requestOptions: null, CancellationToken.None);
    }

    [Theory, AutoNSubstituteData]
    public async Task UpdateAsync_Does_Not_Retry_On_Unrelated_CosmosException(
        string documentId,
        string partitionKey,
        CancellationToken cancellationToken)
    {
        // Arrange
        container
            .ReplaceItemAsync<object>(item: null, id: null, partitionKey: null, requestOptions: null, CancellationToken.None)
            .ReturnsForAnyArgs(Task.FromException<ItemResponse<object>>(CosmosError(HttpStatusCode.NotFound)));

        var act = () => sut.UpdateAsync(
            documentId,
            partitionKey,
            updateDocument: _ => { },
            retries: 5,
            cancellationToken);

        // Act & assert
        await act.Should().ThrowAsync<CosmosException>();

        _ = container
            .ReceivedWithAnyArgs(1)
            .ReplaceItemAsync<object>(item: null, id: null, partitionKey: null, requestOptions: null, CancellationToken.None);
    }

    [Theory, AutoNSubstituteData]
    public async Task UpdateOrCreateAsync_Retries_On_Conflict_And_Rethrows_When_Retries_Exhausted(
        CancellationToken cancellationToken)
    {
        // Arrange (record has an ETag, so the writer takes the Replace path)
        container
            .ReplaceItemAsync<object>(item: null, id: null, partitionKey: null, requestOptions: null, CancellationToken.None)
            .ReturnsForAnyArgs(Task.FromException<ItemResponse<object>>(CosmosError(HttpStatusCode.Conflict)));

        var act = () => sut.UpdateOrCreateAsync(
            () => record,
            updateDocument: _ => { },
            retries: 2,
            cancellationToken);

        // Act & assert
        await act.Should().ThrowAsync<CosmosException>();

        _ = container
            .ReceivedWithAnyArgs(2)
            .ReplaceItemAsync<object>(item: null, id: null, partitionKey: null, requestOptions: null, CancellationToken.None);
    }

    private static CosmosException CosmosError(HttpStatusCode statusCode)
        => new("fake", statusCode, subStatusCode: 0, activityId: "1", requestCharge: 1);
}