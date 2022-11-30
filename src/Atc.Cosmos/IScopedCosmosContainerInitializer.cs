namespace Atc.Cosmos
{
    /// <summary>
    /// Represents a <see cref="IScopedCosmosContainerInitializer"/> scoped to <see cref="CosmosOptions"/>.
    /// </summary>
    public interface IScopedCosmosContainerInitializer
    {
        /// <summary>
        /// Gets the options which the initializer was registered. Null means default options.
        /// </summary>
        CosmosOptions? Scope { get; }

        /// <summary>
        /// Gets the <see cref="ICosmosContainerInitializer"/>.
        /// </summary>
        ICosmosContainerInitializer Initializer { get; }
    }
}