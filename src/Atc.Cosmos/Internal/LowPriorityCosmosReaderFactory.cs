#if PREVIEW
using Atc.Cosmos.Internal;

namespace Atc.Cosmos
{
    public class LowPriorityCosmosReaderFactory : ILowPriorityCosmosReaderFactory
    {
        private readonly ICosmosContainerProvider provider;

        public LowPriorityCosmosReaderFactory(
            ICosmosContainerProvider provider)
        {
            this.provider = provider;
        }

        public ILowPriorityCosmosReader<TResource> CreateReader<TResource>()
            where TResource : class, ICosmosResource
            => new LowPriorityCosmosReader<TResource>(provider);

        public ILowPriorityCosmosBulkReader<TResource> CreateBulkReader<TResource>()
            where TResource : class, ICosmosResource
            => new LowPriorityCosmosBulkReader<TResource>(provider);
    }
}
#endif