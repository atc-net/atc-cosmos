using Atc.Cosmos.Serialization;
using Microsoft.Extensions.Options;

namespace Atc.Cosmos.Internal
{
    public class CosmosReaderFactory : ICosmosReaderFactory
    {
        private readonly ICosmosContainerProvider provider;

        public CosmosReaderFactory(
            ICosmosContainerProvider provider)
        {
            this.provider = provider;
        }

        public ICosmosReader<TResource> CreateReader<TResource>()
            where TResource : class, ICosmosResource
            => new CosmosReader<TResource>(provider);

        public ICosmosBulkReader<TResource> CreateBulkReader<TResource>()
            where TResource : class, ICosmosResource
            => new CosmosBulkReader<TResource>(provider);
    }
}
