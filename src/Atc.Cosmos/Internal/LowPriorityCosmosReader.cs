namespace Atc.Cosmos.Internal;

public class LowPriorityCosmosReader<T>(
    ICosmosContainerProvider containerProvider)
    : CosmosReader<T>(containerProvider), ILowPriorityCosmosReader<T>
    where T : class, ICosmosResource
{
    protected override PriorityLevel PriorityLevel => PriorityLevel.Low;
}