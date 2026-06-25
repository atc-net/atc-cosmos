namespace Atc.Cosmos.Internal;

public class LowPriorityCosmosBulkReader<T>(
    ICosmosContainerProvider containerProvider)
    : CosmosBulkReader<T>(containerProvider), ILowPriorityCosmosBulkReader<T>
    where T : class, ICosmosResource
{
    protected override PriorityLevel PriorityLevel => PriorityLevel.Low;
}