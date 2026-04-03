namespace Atc.Cosmos.Internal;

public class CosmosContainerNameProviderFactory : ICosmosContainerNameProviderFactory
{
    private readonly HashSet<Type> constraints = [];

    public ICosmosContainerNameProvider Register<T>(
        string containerName,
        CosmosOptions? options = null)
        where T : ICosmosResource
    {
        if (HasAlreadyBeenRegistered(typeof(T)))
        {
            throw new NotSupportedException(
                $"Type {typeof(T).Name} can only be registered once.");
        }

        return new CosmosContainerNameProvider<T>(containerName, options);
    }

    public ICosmosContainerNameProvider Register(
        Type resourceType,
        string containerName,
        CosmosOptions? options = null)
    {
        if (HasAlreadyBeenRegistered(resourceType))
        {
            throw new NotSupportedException(
                $"Type {resourceType.Name} can only be registered once.");
        }

        return new CosmosContainerNameProvider(resourceType, containerName, options);
    }

    private bool HasAlreadyBeenRegistered(Type type)
    {
        if (type.IsGenericTypeDefinition || !type.IsGenericType)
        {
            return !constraints.Add(type);
        }

        if (type.IsGenericType &&
            constraints.Contains(type.GetGenericTypeDefinition()))
        {
            // if the generic version has already been registered then go no further.
            return true;
        }

        return !constraints.Add(type);
    }
}