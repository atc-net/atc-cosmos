namespace Atc.Cosmos.Internal;

public class LowPriorityCosmosWriterFactory(
    ICosmosContainerProvider provider,
    ILowPriorityCosmosReaderFactory factory,
    IJsonCosmosSerializer serializer)
    : ILowPriorityCosmosWriterFactory
{
    public ILowPriorityCosmosWriter<TResource> CreateWriter<TResource>()
        where TResource : class, ICosmosResource
        => new LowPriorityCosmosWriter<TResource>(
            provider,
            factory.CreateReader<TResource>(),
            serializer);

    public ILowPriorityCosmosBulkWriter<TResource> CreateBulkWriter<TResource>()
        where TResource : class, ICosmosResource
        => new LowPriorityCosmosBulkWriter<TResource>(provider);
}