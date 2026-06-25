namespace Atc.Cosmos.Internal;

public class CosmosContainerNameProvider(
    Type containerType,
    string containerName,
    CosmosOptions? options)
    : ICosmosContainerNameProvider
{
    public CosmosOptions? Options { get; set; } = options;

    public string ContainerName { get; } = containerName;

    public bool IsForType(Type resourceType)
    {
        if (containerType.IsGenericTypeDefinition)
        {
            if (resourceType.GetGenericTypeDefinition() == containerType)
            {
                return true;
            }
        }
        else if (containerType == resourceType)
        {
            return true;
        }

        return false;
    }
}