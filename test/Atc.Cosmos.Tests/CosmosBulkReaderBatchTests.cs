namespace Atc.Cosmos.Tests;

public sealed class CosmosBulkReaderBatchTests
{
    private readonly FeedIterator<Record> feedIterator;
    private readonly FeedResponse<Record> feedResponse;
    private readonly Record record;
    private readonly Container container;
    private readonly ICosmosContainerProvider containerProvider;
    private readonly CosmosBulkReader<Record> sut;

    public CosmosBulkReaderBatchTests()
    {
        var fixture = FixtureFactory.Create();
        record = fixture.Create<Record>();
        var itemResponse = Substitute.For<ItemResponse<Record>>();

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

        sut = new CosmosBulkReader<Record>(containerProvider);
    }

    [Fact]
    public void Implements_Interface()
        => sut.Should().BeAssignableTo<ICosmosBulkReader<Record>>();

    [Theory, AutoNSubstituteData]
    public void ReadAllAsync_Uses_The_Right_Container(
        string partitionKey,
        CancellationToken cancellationToken)
    {
        // Act
        _ = sut.BatchReadAllAsync(partitionKey, cancellationToken);

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
            .BatchReadAllAsync(partitionKey, cancellationToken)
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
            .BatchReadAllAsync(partitionKey, cancellationToken)
            .ToListAsync(cancellationToken);

        // Assert
        _ = feedIterator
            .Received(2)
            .HasMoreResults;

        _ = feedIterator
            .Received(1)
            .ReadNextAsync(CancellationToken.None);

        response.First().Should().BeEmpty();
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
            .BatchReadAllAsync(partitionKey, cancellationToken)
            .ToListAsync(cancellationToken);

        // Assert
        _ = feedIterator
            .Received(2)
            .HasMoreResults;

        _ = feedIterator
            .Received(1)
            .ReadNextAsync(CancellationToken.None);

        response.Should().NotBeEmpty();
        response[0].First().Should().Be(record);
    }

    [Theory, AutoNSubstituteData]
    public void QueryAsync_Uses_The_Right_Container(
        QueryDefinition query,
        string partitionKey,
        CancellationToken cancellationToken)
    {
        // Act
        _ = sut.BatchQueryAsync(query, partitionKey, cancellationToken);

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
            .BatchQueryAsync(query, partitionKey, cancellationToken)
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
            .BatchQueryAsync(query, partitionKey, cancellationToken)
            .ToListAsync(cancellationToken);

        // Assert
        _ = feedIterator
            .Received(2)
            .HasMoreResults;

        _ = feedIterator
            .Received(1)
            .ReadNextAsync(CancellationToken.None);

        response.First().Should().BeEmpty();
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
            .BatchQueryAsync(query, partitionKey, cancellationToken)
            .ToListAsync(cancellationToken);

        // Assert
        _ = feedIterator
            .Received(2)
            .HasMoreResults;

        _ = feedIterator
            .Received(1)
            .ReadNextAsync(CancellationToken.None);

        response.Should().NotBeEmpty();
        response[0].First().Should().Be(record);
    }

    [Theory, AutoNSubstituteData]
    public void Multiple_Operations_Uses_Same_Container(
        QueryDefinition query,
        string partitionKey,
        CancellationToken cancellationToken)
    {
        // Act
        _ = sut.BatchReadAllAsync(partitionKey, cancellationToken).ToArrayAsync(cancellationToken);
        _ = sut.BatchQueryAsync(query, partitionKey, cancellationToken).ToListAsync(cancellationToken);
        _ = sut.BatchCrossPartitionQueryAsync(query, cancellationToken).ToListAsync(cancellationToken);

        // Assert
        container.ReceivedCalls().Should().HaveCount(3);
    }

    [Theory, AutoNSubstituteData]
    public void QueryAsync_With_Custom_Result_Uses_The_Right_Container(
        QueryDefinition query,
        string partitionKey,
        CancellationToken cancellationToken)
    {
        // Act
        _ = sut.BatchQueryAsync<Record>(query, partitionKey, cancellationToken);

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
            .BatchQueryAsync<Record>(query, partitionKey, cancellationToken)
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
            .BatchQueryAsync<Record>(query, partitionKey, cancellationToken)
            .ToListAsync(cancellationToken);

        // Assert
        _ = feedIterator
            .Received(2)
            .HasMoreResults;

        _ = feedIterator
            .Received(1)
            .ReadNextAsync(CancellationToken.None);

        response.First().Should().BeEmpty();
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
            .BatchQueryAsync<Record>(query, partitionKey, cancellationToken)
            .ToListAsync(cancellationToken);

        // Assert
        _ = feedIterator
            .Received(2)
            .HasMoreResults;

        _ = feedIterator
            .Received(1)
            .ReadNextAsync(CancellationToken.None);

        response.Should().NotBeEmpty();
        response[0].First().Should().Be(record);
    }

    [Theory, AutoNSubstituteData]
    public void CrossPartitionQueryAsync_Uses_The_Right_Container(
        QueryDefinition query,
        CancellationToken cancellationToken)
    {
        // Act
        _ = sut.BatchCrossPartitionQueryAsync(query, cancellationToken);

        // Assert
        containerProvider
            .Received(1)
            .GetContainer<Record>(allowBulk: true);
    }

    [Theory, AutoNSubstituteData]
    public void CrossPartitionQueryAsync_Does_Not_Specify_QueryRequestOptions(
        QueryDefinition query,
        CancellationToken cancellationToken)
    {
        // Act
        _ = sut.BatchCrossPartitionQueryAsync(query, cancellationToken).ToArrayAsync(cancellationToken);

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
            .BatchCrossPartitionQueryAsync(query, cancellationToken)
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
            .BatchCrossPartitionQueryAsync(query, cancellationToken)
            .ToListAsync(cancellationToken);

        // Assert
        _ = feedIterator
            .Received(2)
            .HasMoreResults;

        _ = feedIterator
            .Received(1)
            .ReadNextAsync(CancellationToken.None);

        response.First().Should().BeEmpty();
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
            .BatchCrossPartitionQueryAsync(query, cancellationToken)
            .ToListAsync(cancellationToken);

        // Assert
        _ = feedIterator
            .Received(2)
            .HasMoreResults;

        _ = feedIterator
            .Received(1)
            .ReadNextAsync(CancellationToken.None);

        response.Should().NotBeEmpty();
        response[0].First().Should().Be(record);
    }
}