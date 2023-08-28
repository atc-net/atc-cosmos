#if PREVIEW
using Atc.Cosmos.Internal;
using Microsoft.Azure.Cosmos;

namespace Atc.Cosmos
{
    public class LowPriorityCosmosReader<T>
        : CosmosReader<T>, ILowPriorityCosmosReader<T>
        where T : class, ICosmosResource
    {
        public LowPriorityCosmosReader(ICosmosContainerProvider containerProvider)
            : base(containerProvider)
        {
        }

        protected override PriorityLevel PriorityLevel => PriorityLevel.Low;
    }
}
#endif