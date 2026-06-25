namespace Atc.Cosmos.Internal;

public class LowPriorityCosmosReaderFactory(
    ICosmosContainerProvider provider)
    : ILowPriorityCosmosReaderFactory
{
    public ILowPriorityCosmosReader<TResource> CreateReader<TResource>()
        where TResource : class, ICosmosResource
        => new LowPriorityCosmosReader<TResource>(provider);

    public ILowPriorityCosmosBulkReader<TResource> CreateBulkReader<TResource>()
        where TResource : class, ICosmosResource
        => new LowPriorityCosmosBulkReader<TResource>(provider);
}