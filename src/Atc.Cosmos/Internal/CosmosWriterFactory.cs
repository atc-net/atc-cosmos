using Atc.Cosmos.Serialization;

namespace Atc.Cosmos.Internal
{
    public class CosmosWriterFactory : ICosmosWriterFactory
    {
        private readonly ICosmosContainerProvider provider;
        private readonly IJsonCosmosSerializer serializer;
        private readonly ICosmosReaderFactory factory;

        public CosmosWriterFactory(
            ICosmosContainerProvider provider,
            IJsonCosmosSerializer serializer,
            ICosmosReaderFactory factory)
        {
            this.provider = provider;
            this.serializer = serializer;
            this.factory = factory;
        }

        public ICosmosWriter<TResource> CreateWriter<TResource>()
            where TResource : class, ICosmosResource
            => new CosmosWriter<TResource>(
                provider,
                factory.CreateReader<TResource>(),
                serializer);

        public ICosmosBulkWriter<TResource> CreateBulkWriter<TResource>()
            where TResource : class, ICosmosResource
            => new CosmosBulkWriter<TResource>(
                provider,
                serializer);
    }
}
