using System;

namespace Atc.Cosmos.Internal
{
    /// <summary>
    /// Represents a provider for container names.
    /// </summary>
    public interface ICosmosContainerNameProvider
    {
        /// <summary>
        /// Gets the configured container name for
        /// the specified <see cref="ICosmosResource"/> type.
        /// </summary>
        public string ContainerName { get; }

        /// <summary>
        /// Gets the configured database name for
        /// the specified <see cref="ICosmosResource"/> type.
        /// A value of null means default database.
        /// </summary>
        public string? DatabaseName { get; }

        /// <summary>
        /// Verifies if this provider is for the given resource type or not.
        /// </summary>
        /// <param name="resourceType">The <see cref="Type"/>
        /// of the <see cref="ICosmosResource"/>.</param>
        /// <returns>True if the resource type belongs to this provider; otherwise false.</returns>
        public bool IsForType(Type resourceType);
    }
}