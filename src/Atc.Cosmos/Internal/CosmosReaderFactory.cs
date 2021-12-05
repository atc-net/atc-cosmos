using Atc.Cosmos.Serialization;
using Microsoft.Extensions.Options;

namespace Atc.Cosmos.Internal
{
    public class CosmosReaderFactory : ICosmosReaderFactory
    {
        private readonly ICosmosContainerProvider provider;
        private readonly IOptions<CosmosOptions> options;

        public CosmosReaderFactory(
            ICosmosContainerProvider provider,
            IOptions<CosmosOptions> options)
        {
            this.provider = provider;
            this.options = options;
        }

        public ICosmosReader<TResource> CreateReader<TResource>()
            where TResource : class, ICosmosResource
            => new CosmosReader<TResource>(provider, options);

        public ICosmosBulkReader<TResource> CreateBulkReader<TResource>()
            where TResource : class, ICosmosResource
            => new CosmosBulkReader<TResource>(provider);
    }
}
