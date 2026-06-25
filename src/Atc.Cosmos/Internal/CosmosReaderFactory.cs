namespace Atc.Cosmos.Internal;

public class CosmosReaderFactory(ICosmosContainerProvider provider) : ICosmosReaderFactory
{
    public ICosmosReader<TResource> CreateReader<TResource>()
        where TResource : class, ICosmosResource
        => new CosmosReader<TResource>(provider);

    public ICosmosBulkReader<TResource> CreateBulkReader<TResource>()
        where TResource : class, ICosmosResource
        => new CosmosBulkReader<TResource>(provider);
}