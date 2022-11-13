using System;
using Microsoft.Extensions.DependencyInjection;

namespace Atc.Cosmos.DependencyInjection
{
    /// <summary>
    /// Represents a builder for configuring a Cosmos container.
    /// </summary>
    public interface ICosmosContainerBuilder
    {
        /// <summary>
        /// Gets the service collection.
        /// </summary>
        public IServiceCollection Services { get; }

        /// <summary>
        /// Gets the name of the container being built.
        /// </summary>
        public string ContainerName { get; }

        /// <summary>
        /// Gets the database name for the container being built. Null means default database as specified in <see cref="CosmosOptions"/>.
        /// </summary>
        public string? DatabaseName { get; }

        /// <summary>
        /// Adds a <see cref="ICosmosResource"/> to be used for
        /// representing document resources in the container.
        /// </summary>
        /// <typeparam name="T">
        /// The <see cref="ICosmosResource"/> to be used for
        /// representing document resources in the container.
        /// </typeparam>
        /// <returns>An <see cref="ICosmosContainerBuilder" /> for configuring the container.</returns>
        ICosmosContainerBuilder<T> AddResource<T>()
            where T : class, ICosmosResource;

        /// <summary>
        /// Adds a <see cref="ICosmosResource"/> to be used for
        /// representing document resources in the container.
        /// </summary>
        /// <param name="resourceType">
        /// The <see cref="ICosmosResource"/> to be used for
        /// representing document resources in the container.
        /// </param>
        /// <returns>An <see cref="ICosmosContainerBuilder" /> for configuring the container.</returns>
        ICosmosContainerBuilder AddResource(
            Type resourceType);
    }
}