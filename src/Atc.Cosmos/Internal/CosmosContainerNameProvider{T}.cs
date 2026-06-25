namespace Atc.Cosmos.Internal;

public class CosmosContainerNameProvider<T>(
    string containerName,
    CosmosOptions? options)
    : ICosmosContainerNameProvider
    where T : ICosmosResource
{
    public string ContainerName { get; } = containerName;

    public CosmosOptions? Options { get; set; } = options;

    public bool IsForType(Type resourceType) => typeof(T) == resourceType;
}