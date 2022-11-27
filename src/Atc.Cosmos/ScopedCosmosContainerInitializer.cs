namespace Atc.Cosmos
{
    public class ScopedCosmosContainerInitializer : IScopedCosmosContainerInitializer
    {
        public ScopedCosmosContainerInitializer(CosmosOptions? options, ICosmosContainerInitializer initializer)
        {
            Scope = options;
            Initializer = initializer;
        }

        public CosmosOptions? Scope { get; }

        public ICosmosContainerInitializer Initializer { get; }
    }
}