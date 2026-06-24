namespace Atc.Cosmos;

public class LowPriorityCosmosBulkWriter<T>
    : CosmosBulkWriter<T>, ILowPriorityCosmosBulkWriter<T>
    where T : class, ICosmosResource
{
    public LowPriorityCosmosBulkWriter(
        ICosmosContainerProvider containerProvider,
        IJsonCosmosSerializer serializer)
        : base(containerProvider)
    {
    }

    protected override PriorityLevel PriorityLevel => PriorityLevel.Low;
}