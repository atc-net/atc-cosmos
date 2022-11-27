namespace Atc.Cosmos
{
    public interface IScopedCosmosContainerInitializer
    {
        CosmosOptions? Scope { get; }

        ICosmosContainerInitializer Initializer { get; }
    }
}