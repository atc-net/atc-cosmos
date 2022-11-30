using System;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace Atc.Cosmos.Internal
{
    /// <summary>
    /// Represents a provider for cosmos <see cref="Container"/> instances.
    /// </summary>
    public interface ICosmosContainerProvider
    {
        /// <summary>
        /// Get the configured container for the specified <see cref="ICosmosResource"/> type.
        /// </summary>
        /// <typeparam name="T">The <see cref="ICosmosResource"/>.</typeparam>
        /// <param name="allowBulk">
        /// Boolean indicating if the container should
        /// be configured for bulk operations. Default is false.
        /// </param>
        /// <returns>A cosmos <see cref="Container"/>.</returns>
        Container GetContainer<T>(bool allowBulk = false);

        /// <summary>
        /// Get the configured container for the specified <see cref="ICosmosResource"/> type.
        /// </summary>
        /// <param name="resourceType">
        /// The <see cref="Type"/> of the <see cref="ICosmosResource"/>.
        /// </param>
        /// <param name="allowBulk">
        /// Boolean indicating if the container should
        /// be configured for bulk operations. Default is false.
        /// </param>
        /// <returns>A cosmos <see cref="Container"/>.</returns>
        Container GetContainer(Type resourceType, bool allowBulk = false);

        /// <summary>
        /// Get the configured container for the specified name in the default database.
        /// </summary>
        /// <param name="name">
        /// Name of the container.
        /// </param>
        /// <param name="allowBulk">
        /// Boolean indicating if the container should
        /// be configured for bulk operations.
        /// Default is false.
        /// </param>
        /// <returns>A cosmos <see cref="Container"/>.</returns>
        Container GetContainer(string name, bool allowBulk = false);

        /// <summary>
        /// Get a container for the specified name in the database configured for the type T.
        /// </summary>
        /// <typeparam name="T">The <see cref="ICosmosResource"/>.</typeparam>
        /// <param name="name">The name of the container to be returned.</param>
        /// <param name="allowBulk">
        /// Boolean indicating if the container should
        /// be configured for bulk operations. Default is false.
        /// </param>
        /// <returns>A cosmos <see cref="Container"/>.</returns>
        Container GetContainerWithName<T>(
            string name,
            bool allowBulk = false);

        /// <summary>
        /// Get a container for the specified name in the database configured for the type T.
        /// </summary>
        /// <param name="resourceType">
        /// The <see cref="Type"/> of the <see cref="ICosmosResource"/>.
        /// </param>
        /// <param name="name">The name of the container to be returned.</param>
        /// <param name="allowBulk">
        /// Boolean indicating if the container should
        /// be configured for bulk operations. Default is false.
        /// </param>
        /// <returns>A cosmos <see cref="Container"/>.</returns>
        Container GetContainerWithName(
                    Type resourceType,
                    string name,
                    bool allowBulk = false);

        /// <summary>
        /// Gets the options associated with the type T.
        /// If no options registered with the type T, default options are returned.
        /// </summary>
        /// <typeparam name="T">The resource type.</typeparam>
        /// <returns>The associated <see cref="CosmosOptions"/>.</returns>
        CosmosOptions GetCosmosOptions<T>();

        /// <summary>
        /// Gets the options associated with the type resourceType.
        /// If no options registered with the type resourceType, default options are returned.
        /// </summary>
        /// <param name="resourceType">The resource type.</param>
        /// <returns>The associated <see cref="CosmosOptions"/>.</returns>
        CosmosOptions GetCosmosOptions(Type resourceType);
    }
}