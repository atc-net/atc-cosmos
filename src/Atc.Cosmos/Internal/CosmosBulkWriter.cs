namespace Atc.Cosmos.Internal;

public class CosmosBulkWriter<T> : ICosmosBulkWriter<T>
    where T : class, ICosmosResource
{
    private readonly Container container;

    public CosmosBulkWriter(ICosmosContainerProvider containerProvider)
        => container = containerProvider.GetContainer<T>(allowBulk: true);

    protected virtual PriorityLevel PriorityLevel => PriorityLevel.High;

    public Task CreateAsync(
        T document,
        CancellationToken cancellationToken = default)
        => container
            .CreateItemAsync<object>(
                document,
                new PartitionKey(document.PartitionKey),
                new ItemRequestOptions
                {
                    EnableContentResponseOnWrite = false,
                    PriorityLevel = PriorityLevel,
                },
                cancellationToken);

    public Task WriteAsync(
        T document,
        CancellationToken cancellationToken = default)
        => container
            .UpsertItemAsync<object>(
                document,
                new PartitionKey(document.PartitionKey),
                new ItemRequestOptions
                {
                    EnableContentResponseOnWrite = false,
                    PriorityLevel = PriorityLevel,
                },
                cancellationToken);

    public Task ReplaceAsync(
        T document,
        CancellationToken cancellationToken = default)
        => container
            .ReplaceItemAsync<object>(
                document,
                document.DocumentId,
                new PartitionKey(document.PartitionKey),
                new ItemRequestOptions
                {
                    IfMatchEtag = document.ETag,
                    EnableContentResponseOnWrite = false,
                    PriorityLevel = PriorityLevel,
                },
                cancellationToken);

    public Task DeleteAsync(
        string documentId,
        string partitionKey,
        CancellationToken cancellationToken = default)
        => container
            .DeleteItemAsync<object>(
                documentId,
                new PartitionKey(partitionKey),
                new ItemRequestOptions
                {
                    EnableContentResponseOnWrite = false,
                    PriorityLevel = PriorityLevel,
                },
                cancellationToken: cancellationToken);
}