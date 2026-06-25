namespace Atc.Cosmos.Internal;

public class LowPriorityCosmosBulkWriter<T>(
    ICosmosContainerProvider containerProvider)
    : CosmosBulkWriter<T>(containerProvider), ILowPriorityCosmosBulkWriter<T>
    where T : class, ICosmosResource
{
    protected override PriorityLevel PriorityLevel => PriorityLevel.Low;
}