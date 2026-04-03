namespace SampleApi;

public class FooService(
    ICosmosReader<FooResource> reader,
    ICosmosWriter<FooResource> writer)
{
    public Task<FooResource?> FindAsync(
        string id,
        CancellationToken cancellationToken = default) =>
        reader.FindAsync(id, FooResource.PartitionKey, cancellationToken);

    public Task UpsertAsync(
        string? id = null,
        Dictionary<string, object>? data = null,
        CancellationToken cancellationToken = default) =>
        writer.UpdateOrCreateAsync(
            () => new FooResource { Id = id ?? Guid.NewGuid().ToString() },
            resource => resource.Data = data ?? new Dictionary<string, object>(StringComparer.Ordinal),
            retries: 5,
            cancellationToken);
}