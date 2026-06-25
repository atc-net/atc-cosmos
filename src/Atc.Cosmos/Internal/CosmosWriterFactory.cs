namespace Atc.Cosmos.Internal;

public class CosmosWriterFactory(
    ICosmosContainerProvider provider,
    IJsonCosmosSerializer serializer,
    ICosmosReaderFactory factory)
    : ICosmosWriterFactory
{
    public ICosmosWriter<TResource> CreateWriter<TResource>()
        where TResource : class, ICosmosResource
        => new CosmosWriter<TResource>(
            provider,
            factory.CreateReader<TResource>(),
            serializer);

    public ICosmosBulkWriter<TResource> CreateBulkWriter<TResource>()
        where TResource : class, ICosmosResource
        => new CosmosBulkWriter<TResource>(
            provider);
}