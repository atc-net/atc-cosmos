#if PREVIEW
using Atc.Cosmos.Internal;
using Microsoft.Azure.Cosmos;

namespace Atc.Cosmos
{
    public class LowPriorityCosmosBulkReader<T>
        : CosmosBulkReader<T>, ILowPriorityCosmosBulkReader<T>
        where T : class, ICosmosResource
    {
        public LowPriorityCosmosBulkReader(ICosmosContainerProvider containerProvider)
            : base(containerProvider)
        {
        }

        protected override PriorityLevel PriorityLevel => PriorityLevel.Low;
    }
}
#endif