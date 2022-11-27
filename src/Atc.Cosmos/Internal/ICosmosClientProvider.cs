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
        /// <param name="options">The <see cref="CosmosOptions"/> to use for creating a <see cref="CosmosClient"/></param>
        /// <returns>A <see cref="CosmosClient"/> instance.</returns>
        CosmosClient GetClient(CosmosOptions options);

        /// <summary>
        /// Get the <see cref="CosmosClient"/> instance configured for bulk operations.
        /// </summary>
        /// <param name="options">The <see cref="CosmosOptions"/> to use for creating a <see cref="CosmosClient"/></param>
        /// <returns><see cref="CosmosClient"/> instance.</returns>
        CosmosClient GetBulkClient(CosmosOptions options);
    }
}