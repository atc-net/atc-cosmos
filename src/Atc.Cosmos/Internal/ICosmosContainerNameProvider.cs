using System;

namespace Atc.Cosmos.Internal
{
    /// <summary>
    /// Represents a provider for container names.
    /// </summary>
    public interface ICosmosContainerNameProvider
    {
        /// <summary>
        /// Resolves the configured container name for
        /// the specified <see cref="ICosmosResource"/> type.
        /// </summary>
        /// <param name="resourceType">The <see cref="Type"/>
        /// of the <see cref="ICosmosResource"/>.</param>
        /// <returns>A container name.</returns>
        string? GetContainerName(
            Type resourceType);
    }
}