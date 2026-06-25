namespace Atc.Cosmos.Internal;

public class CosmosContainerProvider(
    ICosmosClientProvider clientProvider,
    ICosmosContainerRegistry registry)
    : ICosmosContainerProvider
{
    public Container GetContainer<T>(bool allowBulk = false)
    {
        var container = registry.GetContainerForType<T>();
        var options = container.Options ?? registry.DefaultOptions;

        return GetClient(options, allowBulk)
            .GetContainer(
                options.DatabaseName,
                container.ContainerName);
    }

    public Container GetContainer(
        Type resourceType,
        bool allowBulk = false)
    {
        var container = registry.GetContainerForType(resourceType);
        var options = container.Options ?? registry.DefaultOptions;

        return GetClient(options, allowBulk)
            .GetContainer(
                options.DatabaseName,
                container.ContainerName);
    }

    public Container GetContainer(
        string name,
        bool allowBulk = false)
        => GetClient(registry.DefaultOptions, allowBulk)
            .GetContainer(
                registry.DefaultOptions.DatabaseName,
                name);

    public Container GetContainerWithName<T>(
        string name,
        bool allowBulk = false)
    {
        var container = registry.GetContainerForType<T>();
        var options = container.Options ?? registry.DefaultOptions;

        return GetClient(options, allowBulk)
            .GetContainer(
                options.DatabaseName,
                name);
    }

    public Container GetContainerWithName(
        Type resourceType,
        string name,
        bool allowBulk = false)
    {
        var container = registry.GetContainerForType(resourceType);
        var options = container.Options!;

        return GetClient(options, allowBulk)
            .GetContainer(
                options.DatabaseName,
                name);
    }

    public CosmosOptions GetCosmosOptions<T>()
        => registry
            .GetContainerForType<T>()
            .Options!;

    public CosmosOptions GetCosmosOptions(Type resourceType)
        => registry
            .GetContainerForType(resourceType)
            .Options!;

    private CosmosClient GetClient(
        CosmosOptions options,
        bool allowBulk)
        => allowBulk
            ? clientProvider.GetBulkClient(options)
            : clientProvider.GetClient(options);
}