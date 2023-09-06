#if PREVIEW
using Atc.Cosmos.Internal;
using Atc.Cosmos.Serialization;

namespace Atc.Cosmos
{
    public class LowPriorityCosmosWriterFactory : ILowPriorityCosmosWriterFactory
    {
        private readonly ICosmosContainerProvider provider;
        private readonly ILowPriorityCosmosReaderFactory factory;
        private readonly IJsonCosmosSerializer serializer;

        public LowPriorityCosmosWriterFactory(
            ICosmosContainerProvider provider,
            ILowPriorityCosmosReaderFactory factory,
            IJsonCosmosSerializer serializer)
        {
            this.provider = provider;
            this.factory = factory;
            this.serializer = serializer;
        }

        public ILowPriorityCosmosWriter<TResource> CreateWriter<TResource>()
            where TResource : class, ICosmosResource
            => new LowPriorityCosmosWriter<TResource>(
                provider,
                factory.CreateReader<TResource>(),
                serializer);

        public ILowPriorityCosmosBulkWriter<TResource> CreateBulkWriter<TResource>()
            where TResource : class, ICosmosResource
            => new LowPriorityCosmosBulkWriter<TResource>(provider, serializer);
    }
}
#endif