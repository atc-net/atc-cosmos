namespace Atc.Cosmos.Tests;

public sealed class LowPriorityCosmosBulkReaderTests
{
    private readonly ItemResponse<Record> itemResponse;
    private readonly FeedIterator<Record> feedIterator;
    private readonly FeedResponse<Record> feedResponse;
    private readonly Record record;
    private readonly Container container;
    private readonly ICosmosContainerProvider containerProvider;
    private readonly StreamReadStub<Record> streamRead;
    private readonly LowPriorityCosmosBulkReader<Record> sut;

    public LowPriorityCosmosBulkReaderTests()
    {
        record = new Fixture().Create<Record>();

        itemResponse = Substitute.For<ItemResponse<Record>>();

        itemResponse
            .Resource
            .Returns(record);

        feedResponse = Substitute.For<FeedResponse<Record>>();
        feedIterator = Substitute.For<FeedIterator<Record>>();

        feedIterator
            .ReadNextAsync(CancellationToken.None)
            .ReturnsForAnyArgs(feedResponse);

        container = Substitute.For<Container>();

        container
            .ReadItemAsync<Record>(id: null, partitionKey: default, requestOptions: null)
            .ReturnsForAnyArgs(itemResponse);

        container
            .GetItemQueryIterator<Record>(queryDefinition: null, continuationToken: null)
            .ReturnsForAnyArgs(feedIterator);

        container
            .GetItemQueryIterator<Record>(queryText: null, continuationToken: null)
            .ReturnsForAnyArgs(feedIterator);

        containerProvider = Substitute.For<ICosmosContainerProvider>();

        containerProvider
            .GetContainer<Record>(allowBulk: true)
            .Returns(container, null);

        streamRead = new StreamReadStub<Record>(container, record);

        sut = new LowPriorityCosmosBulkReader<Record>(containerProvider);
    }

    [Fact]
    public void Implements_Interface()
        => sut.Should().BeAssignableTo<ILowPriorityCosmosBulkReader<Record>>();

    [Theory, AutoNSubstituteData]
    public async Task ReadAsync_Uses_The_Right_Container(
        string documentId,
        string partitionKey,
        CancellationToken cancellationToken)
    {
        // Act
        await sut.ReadAsync(documentId, partitionKey, cancellationToken);

        // Assert
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
        // Act
        await sut.ReadAsync(documentId, partitionKey, cancellationToken);

        // Assert
        _ = container
            .Received(1)
            .ReadItemAsync<Record>(
                documentId,
                new PartitionKey(partitionKey),
                Arg.Is<ItemRequestOptions>(c => c.PriorityLevel == PriorityLevel.Low),
                cancellationToken);
    }

    [Theory, AutoNSubstituteData]
    public async Task FindAsync_Reads_Item_In_Container_Using_PriorityLevel_Low(
        string documentId,
        string partitionKey,
        CancellationToken cancellationToken)
    {
        // Act
        await sut.FindAsync(documentId, partitionKey, cancellationToken);

        // Assert
        _ = container
            .Received(1)
            .ReadItemStreamAsync(
                documentId,
                new PartitionKey(partitionKey),
                Arg.Is<ItemRequestOptions>(c => c.PriorityLevel == PriorityLevel.Low),
                cancellationToken);
    }

    [Theory, AutoNSubstituteData]
    public async Task ReadAsync_Returns_Item_Read_From_Container(
        string documentId,
        string partitionKey,
        CancellationToken cancellationToken)
    {
        // Act
        var result = await sut.ReadAsync(documentId, partitionKey, cancellationToken);

        // Assert
        result.Should().Be(itemResponse.Resource);
    }

    [Theory, AutoNSubstituteData]
    public Task ReadAsync_Throws_Exception_When_Record_Is_Not_Found(
        CosmosException exception,
        string documentId,
        string partitionKey,
        CancellationToken cancellationToken)
    {
        // Arrange
        container
            .ReadItemAsync<Record>(id: null, partitionKey: default, requestOptions: null, CancellationToken.None)
            .ReturnsForAnyArgs(Task.FromException<ItemResponse<Record>>(exception));

        // Act & assert
        return FluentActions.Awaiting(() => sut.ReadAsync(documentId, partitionKey, cancellationToken))
            .Should()
            .ThrowAsync<CosmosException>();
    }

    [Theory, AutoNSubstituteData]
    public async Task FindAsync_Uses_The_Right_Container(
        string documentId,
        string partitionKey,
        CancellationToken cancellationToken)
    {
        // Act
        await sut.FindAsync(documentId, partitionKey, cancellationToken);

        // Assert
        containerProvider
            .Received(1)
            .GetContainer<Record>(allowBulk: true);
    }

    [Theory, AutoNSubstituteData]
    public async Task FindAsync_Returns_Default_When_Record_Is_Not_Found(
        string documentId,
        string partitionKey,
        CancellationToken cancellationToken)
    {
        // Arrange
        streamRead.StatusCode = HttpStatusCode.NotFound;

        // Act
        var response = await sut.FindAsync(documentId, partitionKey, cancellationToken);

        // Assert
        response.Should().BeNull();
    }

    [Theory, AutoNSubstituteData]
    public async Task FindAsync_Returns_Default_When_Container_Throws(
        CosmosException exception,
        string documentId,
        string partitionKey,
        CancellationToken cancellationToken)
    {
        // Arrange
        container
            .ReadItemStreamAsync(id: null, partitionKey: default, requestOptions: null, CancellationToken.None)
            .ReturnsForAnyArgs(Task.FromException<ResponseMessage>(exception));

        // Act
        var response = await sut.FindAsync(documentId, partitionKey, cancellationToken);

        // Assert
        response.Should().BeNull();
    }

    [Theory, AutoNSubstituteData]
    public async Task FindAsync_Returns_Record_When_Successful(
        string partitionKey,
        string documentId,
        CancellationToken cancellationToken)
    {
        // Act
        var result = await sut.FindAsync(documentId, partitionKey, cancellationToken);

        // Assert
        result.Should().Be(record);
    }

    [Theory, AutoNSubstituteData]
    public void ReadAllAsync_Uses_The_Right_Container(
        string partitionKey,
        CancellationToken cancellationToken)
    {
        // Act
        _ = sut.ReadAllAsync(partitionKey, cancellationToken);

        // Assert
        containerProvider
            .Received(1)
            .GetContainer<Record>(allowBulk: true);
    }

    [Theory, AutoNSubstituteData]
    public async Task ReadAllAsync_Returns_Empty_No_More_Result(
        string partitionKey,
        CancellationToken cancellationToken)
    {
        // Arrange
        feedIterator.HasMoreResults.Returns(false);

        // Act
        var response = await sut
            .ReadAllAsync(partitionKey, cancellationToken)
            .ToListAsync(cancellationToken);

        // Assert
        _ = feedIterator
            .Received(1)
            .HasMoreResults;

        _ = feedIterator
            .Received(0)
            .ReadNextAsync(CancellationToken.None);

        response.Should().BeEmpty();
    }

    [Theory, AutoNSubstituteData]
    public async Task ReadAllAsync_Returns_Empty_When_Query_Matches_Non(
        string partitionKey,
        CancellationToken cancellationToken)
    {
        // Arrange
        feedIterator.HasMoreResults.Returns(true, false);

        // Act
        var response = await sut
            .ReadAllAsync(partitionKey, cancellationToken)
            .ToListAsync(cancellationToken);

        // Assert
        _ = feedIterator
            .Received(2)
            .HasMoreResults;

        _ = feedIterator
            .Received(1)
            .ReadNextAsync(CancellationToken.None);

        response.Should().BeEmpty();
    }

    [Theory, AutoNSubstituteData]
    public async Task ReadAllAsync_Returns_All_Items(
        string partitionKey,
        CancellationToken cancellationToken)
    {
        // Arrange
        feedIterator
            .HasMoreResults
            .Returns(true, false);

        using var enumerator = feedResponse.GetEnumerator();
        enumerator.Returns(new List<Record> { record }.GetEnumerator());

        // Act
        var response = await sut
            .ReadAllAsync(partitionKey, cancellationToken)
            .ToListAsync(cancellationToken);

        // Assert
        _ = feedIterator
            .Received(2)
            .HasMoreResults;

        _ = feedIterator
            .Received(1)
            .ReadNextAsync(CancellationToken.None);

        response.Should().NotBeEmpty();
        response[0].Should().Be(record);
    }

    [Theory, AutoNSubstituteData]
    public void QueryAsync_Uses_The_Right_Container(
        QueryDefinition query,
        string partitionKey,
        CancellationToken cancellationToken)
    {
        // Act
        _ = sut.QueryAsync(query, partitionKey, cancellationToken);

        // Assert
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
        // Arrange
        feedIterator.HasMoreResults.Returns(false);

        // Act
        var response = await sut
            .QueryAsync(query, partitionKey, cancellationToken)
            .ToListAsync(cancellationToken);

        // Assert
        _ = feedIterator
            .Received(1)
            .HasMoreResults;

        _ = feedIterator
            .Received(0)
            .ReadNextAsync(CancellationToken.None);

        response.Should().BeEmpty();
    }

    [Theory, AutoNSubstituteData]
    public async Task QueryAsync_Returns_Empty_When_Query_Matches_Non(
        QueryDefinition query,
        string partitionKey,
        CancellationToken cancellationToken)
    {
        // Arrange
        feedIterator.HasMoreResults.Returns(true, false);

        // Act
        var response = await sut
            .QueryAsync(query, partitionKey, cancellationToken)
            .ToListAsync(cancellationToken);

        // Assert
        _ = feedIterator
            .Received(2)
            .HasMoreResults;

        _ = feedIterator
            .Received(1)
            .ReadNextAsync(CancellationToken.None);

        response.Should().BeEmpty();
    }

    [Theory, AutoNSubstituteData]
    public async Task QueryAsync_Returns_Items_When_Query_Matches(
        QueryDefinition query,
        string partitionKey,
        CancellationToken cancellationToken)
    {
        // Arrange
        feedIterator
            .HasMoreResults
            .Returns(true, false);

        using var enumerator = feedResponse.GetEnumerator();
        enumerator.Returns(new List<Record> { record }.GetEnumerator());

        // Act
        var response = await sut
            .QueryAsync(query, partitionKey, cancellationToken)
            .ToListAsync(cancellationToken);

        // Assert
        _ = feedIterator
            .Received(2)
            .HasMoreResults;

        _ = feedIterator
            .Received(1)
            .ReadNextAsync(CancellationToken.None);

        response.Should().NotBeEmpty();
        response[0].Should().Be(record);
    }

    [Theory, AutoNSubstituteData]
    public async Task Should_Have_ETag_From_ItemResponse(
        string etag,
        string partitionKey,
        string documentId,
        CancellationToken cancellationToken)
    {
        // Arrange
        streamRead.ETag = etag;

        // Act
        var result = await sut.FindAsync(documentId, partitionKey, cancellationToken);

        // Assert
        var resource = (ICosmosResource)result;

        resource.Should().NotBeNull();
        resource!.ETag.Should().NotBeNullOrWhiteSpace();
        resource.ETag.Should().Be(etag);
    }

    [Theory, AutoNSubstituteData]
    public async Task Multiple_Operations_Uses_Same_Container(
        QueryDefinition query,
        string documentId,
        string partitionKey,
        CancellationToken cancellationToken)
    {
        // Act
        _ = sut.ReadAsync(documentId, partitionKey, cancellationToken);
        _ = sut.ReadAsync(documentId, partitionKey, cancellationToken);
        _ = sut.FindAsync(documentId, partitionKey, cancellationToken);
        _ = sut.FindAsync(documentId, partitionKey, cancellationToken);

        _ = await sut
            .QueryAsync(query, partitionKey, cancellationToken)
            .ToListAsync(cancellationToken);

        _ = await sut
            .QueryAsync(query, partitionKey, cancellationToken)
            .ToListAsync(cancellationToken);

        // Assert
        container
            .ReceivedCalls()
            .Where(c => c.GetMethodInfo().Name != "get_Database")
            .Should()
            .HaveCount(6);
    }

    [Theory, AutoNSubstituteData]
    public void QueryAsync_With_Custom_Result_Uses_The_Right_Container(
        QueryDefinition query,
        string partitionKey,
        CancellationToken cancellationToken)
    {
        // Act
        _ = sut.QueryAsync<Record>(query, partitionKey, cancellationToken);

        // Assert
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
        // Arrange
        feedIterator.HasMoreResults.Returns(false);

        // Act
        var response = await sut
            .QueryAsync<Record>(query, partitionKey, cancellationToken)
            .ToListAsync(cancellationToken);

        // Assert
        _ = feedIterator
            .Received(1)
            .HasMoreResults;

        _ = feedIterator
            .Received(0)
            .ReadNextAsync(CancellationToken.None);

        response.Should().BeEmpty();
    }

    [Theory, AutoNSubstituteData]
    public async Task QueryAsync_With_Custom_Returns_Empty_When_Query_Matches_Non(
        QueryDefinition query,
        string partitionKey,
        CancellationToken cancellationToken)
    {
        // Arrange
        feedIterator.HasMoreResults.Returns(true, false);

        // Act
        var response = await sut
            .QueryAsync<Record>(query, partitionKey, cancellationToken)
            .ToListAsync(cancellationToken);

        // Assert
        _ = feedIterator
            .Received(2)
            .HasMoreResults;

        _ = feedIterator
            .Received(1)
            .ReadNextAsync(CancellationToken.None);

        response.Should().BeEmpty();
    }

    [Theory, AutoNSubstituteData]
    public async Task QueryAsync_With_Custom_Returns_Items_When_Query_Matches(
        QueryDefinition query,
        string partitionKey,
        CancellationToken cancellationToken)
    {
        // Arrange
        feedIterator
            .HasMoreResults
            .Returns(true, false);

        using var enumerator = feedResponse.GetEnumerator();
        enumerator.Returns(new List<Record> { record }.GetEnumerator());

        // Act
        var response = await sut
            .QueryAsync<Record>(query, partitionKey, cancellationToken)
            .ToListAsync(cancellationToken);

        // Assert
        _ = feedIterator
            .Received(2)
            .HasMoreResults;

        _ = feedIterator
            .Received(1)
            .ReadNextAsync(CancellationToken.None);

        response.Should().NotBeEmpty();
        response[0].Should().Be(record);
    }

    [Theory, AutoNSubstituteData]
    public void CrossPartitionQueryAsync_Uses_The_Right_Container(
        QueryDefinition query,
        CancellationToken cancellationToken)
    {
        // Act
        _ = sut.CrossPartitionQueryAsync(query, cancellationToken);

        // Assert
        containerProvider
            .Received(1)
            .GetContainer<Record>(allowBulk: true);
    }

    [Theory, AutoNSubstituteData]
    public async Task CrossPartitionQueryAsync_Does_Not_Specify_QueryRequestOptions(
        QueryDefinition query,
        CancellationToken cancellationToken)
    {
        // Act
        _ = await sut.CrossPartitionQueryAsync(query, cancellationToken).ToArrayAsync(cancellationToken);

        // Assert
        container
            .Received(1)
            .GetItemQueryIterator<Record>(query, requestOptions: null);
    }

    [Theory, AutoNSubstituteData]
    public async Task CrossPartitionQueryAsync_Returns_Empty_No_More_Result(
        QueryDefinition query,
        CancellationToken cancellationToken)
    {
        // Arrange
        feedIterator.HasMoreResults.Returns(false);

        // Act
        var response = await sut
            .CrossPartitionQueryAsync(query, cancellationToken)
            .ToListAsync(cancellationToken);

        // Assert
        _ = feedIterator
            .Received(1)
            .HasMoreResults;

        _ = feedIterator
            .Received(0)
            .ReadNextAsync(CancellationToken.None);

        response.Should().BeEmpty();
    }

    [Theory, AutoNSubstituteData]
    public async Task CrossPartitionQueryAsync_Returns_Empty_When_Query_Matches_Non(
        QueryDefinition query,
        CancellationToken cancellationToken)
    {
        // Arrange
        feedIterator.HasMoreResults.Returns(true, false);

        // Act
        var response = await sut
            .CrossPartitionQueryAsync(query, cancellationToken)
            .ToListAsync(cancellationToken);

        // Assert
        _ = feedIterator
            .Received(2)
            .HasMoreResults;

        _ = feedIterator
            .Received(1)
            .ReadNextAsync(CancellationToken.None);

        response.Should().BeEmpty();
    }

    [Theory, AutoNSubstituteData]
    public async Task CrossPartitionQueryAsync_Returns_Items_When_Query_Matches(
        QueryDefinition query,
        CancellationToken cancellationToken)
    {
        // Arrange
        feedIterator
            .HasMoreResults
            .Returns(true, false);

        using var enumerator = feedResponse.GetEnumerator();
        enumerator.Returns(new List<Record> { record }.GetEnumerator());

        // Act
        var response = await sut
            .CrossPartitionQueryAsync(query, cancellationToken)
            .ToListAsync(cancellationToken);

        // Assert
        _ = feedIterator
            .Received(2)
            .HasMoreResults;

        _ = feedIterator
            .Received(1)
            .ReadNextAsync(CancellationToken.None);

        response.Should().NotBeEmpty();
        response[0].Should().Be(record);
    }

    [Theory, AutoNSubstituteData]
    public void CrossPartitionPagedQueryAsync_Uses_The_Right_Container(
        QueryDefinition query,
        int pageSize,
        string continuationToken,
        CancellationToken cancellationToken)
    {
        // Act
        _ = sut.CrossPartitionPagedQueryAsync(
            query,
            pageSize,
            continuationToken,
            cancellationToken);

        // Assert
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
        // Act
        _ = sut.CrossPartitionPagedQueryAsync(
            query,
            pageSize,
            continuationToken,
            cancellationToken);

        // Assert
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
        // Arrange
        feedIterator.HasMoreResults.Returns(false);

        // Act
        var response = await sut
            .CrossPartitionPagedQueryAsync(
                query,
                pageSize,
                continuationToken,
                cancellationToken);

        // Assert
        _ = feedIterator
            .Received(1)
            .HasMoreResults;

        _ = feedIterator
            .Received(0)
            .ReadNextAsync(CancellationToken.None);

        response.Items.Should().BeEmpty();
        response.ContinuationToken.Should().BeNull();
    }

    [Theory, AutoNSubstituteData]
    public async Task CrossPartitionPagedQueryAsync_Returns_Items_When_Query_Matches(
        QueryDefinition query,
        int pageSize,
        string continuationToken,
        List<Record> records,
        CancellationToken cancellationToken)
    {
        // Arrange
        feedIterator
            .HasMoreResults
            .Returns(true);

        feedResponse
            .ContinuationToken
            .Returns(continuationToken);

        using var enumerator = feedResponse.GetEnumerator();
        enumerator.Returns(records.GetEnumerator());

        // Act
        var response = await sut
            .CrossPartitionPagedQueryAsync(
                query,
                pageSize,
                null,
                cancellationToken);

        // Assert
        _ = feedIterator
            .Received(1)
            .HasMoreResults;

        _ = feedIterator
            .Received(1)
            .ReadNextAsync(CancellationToken.None);

        response.Items.Should().BeEquivalentTo(records);
        response.ContinuationToken.Should().Be(continuationToken);
    }

    [Theory, AutoNSubstituteData]
    public void CrossPartitionPagedQueryAsync_With_Custom_Uses_The_Right_Container(
        QueryDefinition query,
        int pageSize,
        string continuationToken,
        CancellationToken cancellationToken)
    {
        // Act
        _ = sut.CrossPartitionPagedQueryAsync<Record>(
            query,
            pageSize,
            continuationToken,
            cancellationToken);

        // Assert
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
        // Arrange
        feedIterator.HasMoreResults.Returns(false);

        // Act
        var response = await sut
            .CrossPartitionPagedQueryAsync<Record>(
                query,
                pageSize,
                continuationToken,
                cancellationToken);

        // Assert
        _ = feedIterator
            .Received(1)
            .HasMoreResults;

        _ = feedIterator
            .Received(0)
            .ReadNextAsync(CancellationToken.None);

        response.Items.Should().BeEmpty();
        response.ContinuationToken.Should().BeNull();
    }

    [Theory, AutoNSubstituteData]
    public async Task CrossPartitionPagedQueryAsync_With_Custom_Returns_Items_When_Query_Matches(
        QueryDefinition query,
        int pageSize,
        string continuationToken,
        List<Record> records,
        CancellationToken cancellationToken)
    {
        // Arrange
        feedIterator
            .HasMoreResults
            .Returns(true);

        feedResponse
            .ContinuationToken
            .Returns(continuationToken);

        using var enumerator = feedResponse.GetEnumerator();
        enumerator.Returns(records.GetEnumerator());

        // Act
        var response = await sut
            .CrossPartitionPagedQueryAsync<Record>(
                query,
                pageSize,
                null,
                cancellationToken);

        // Assert
        _ = feedIterator
            .Received(1)
            .HasMoreResults;

        _ = feedIterator
            .Received(1)
            .ReadNextAsync(CancellationToken.None);

        response.Items.Should().BeEquivalentTo(records);
        response.ContinuationToken.Should().Be(continuationToken);
    }
}