using Microsoft.Azure.Cosmos;

namespace Atc.Cosmos.Internal
{
    /// <summary>
    /// Represents a provider for configured
    /// <see cref="CosmosClient"/> instances.
    /// </summary>
    public interface ICosmosClientProvider
    {
        /// <summary>
        /// Get the default <see cref="CosmosClient"/> instance.
        /// </summary>
        /// <returns>A <see cref="CosmosClient"/> instance.</returns>
        CosmosClient GetClient();

        /// <summary>
        /// Get the <see cref="CosmosClient"/> instance configured for bulk operations.
        /// </summary>
        /// <returns><see cref="CosmosClient"/> instance.</returns>
        CosmosClient GetBulkClient();
    }
}