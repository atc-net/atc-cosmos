using System;
using Microsoft.Azure.Cosmos;

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
        /// Get the container with a specified name.
        /// </summary>
        /// <param name="name">The name of the container.</param>
        /// <param name="allowBulk">
        /// Boolean indicating if the container should
        /// be configured for bulk operations. Default is false.
        /// </param>
        /// <returns>A cosmos <see cref="Container"/>.</returns>
        Container GetContainer(string name, bool allowBulk = false);
    }
}