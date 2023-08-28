#if PREVIEW
using Atc.Cosmos.Internal;
using Atc.Cosmos.Serialization;
using Microsoft.Azure.Cosmos;

namespace Atc.Cosmos
{
    public class LowPriorityCosmosWriter<T>
        : CosmosWriter<T>, ILowPriorityCosmosWriter<T>
        where T : class, ICosmosResource
    {
        public LowPriorityCosmosWriter(
            ICosmosContainerProvider containerProvider,
            ILowPriorityCosmosReader<T> reader,
            IJsonCosmosSerializer serializer)
            : base(containerProvider, reader, serializer)
        {
        }

        protected override PriorityLevel PriorityLevel => PriorityLevel.Low;
    }
}
#endif