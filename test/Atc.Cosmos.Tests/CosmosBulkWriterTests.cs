namespace Atc.Cosmos.Tests;

public sealed class CosmosBulkWriterTests
{
    private readonly Record record;
    private readonly Container container;
    private readonly ICosmosContainerProvider containerProvider;
    private readonly CosmosBulkWriter<Record> sut;

    public CosmosBulkWriterTests()
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

        var serializer = Substitute.For<IJsonCosmosSerializer>();

        serializer
            .FromString<Record>(json: Arg.Any<string>())
            .ReturnsForAnyArgs(new Fixture().Create<Record>());

        sut = new CosmosBulkWriter<Record>(containerProvider);
    }

    [Fact]
    public void Implements_Interface()
        => sut.Should().BeAssignableTo<ICosmosBulkWriter<Record>>();

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
                allowBulk: true);
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
                Arg.Is<ItemRequestOptions>(o => o.EnableContentResponseOnWrite == false),
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
                Arg.Is<ItemRequestOptions>(o => o.EnableContentResponseOnWrite == false),
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
                Arg.Is<ItemRequestOptions>(o =>
                    o.EnableContentResponseOnWrite == false &&
                    o.IfMatchEtag == record.ETag),
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
                Arg.Is<ItemRequestOptions>(o => o.EnableContentResponseOnWrite == false),
                cancellationToken: cancellationToken);
    }
}