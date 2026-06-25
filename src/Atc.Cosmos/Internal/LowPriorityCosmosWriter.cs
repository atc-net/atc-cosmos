namespace Atc.Cosmos.Internal;

public class LowPriorityCosmosWriter<T>(
    ICosmosContainerProvider containerProvider,
    ILowPriorityCosmosReader<T> reader,
    IJsonCosmosSerializer serializer)
    : CosmosWriter<T>(containerProvider, reader, serializer), ILowPriorityCosmosWriter<T>
    where T : class, ICosmosResource
{
    protected override PriorityLevel PriorityLevel => PriorityLevel.Low;
}