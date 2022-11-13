using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Atc.Cosmos.DependencyInjection
{
    /// <summary>
    /// Represents a builder for configuring Cosmos.
    /// </summary>
    public interface ICosmosBuilder
    {
        /// <summary>
        /// Gets the service collection.
        /// </summary>
        IServiceCollection Services { get; }

        /// <summary>
        /// Gets the database name related to this builder. Null means default database as defined in <see cref="CosmosOptions.DatabaseName"/>.
        /// </summary>
        string? DatabaseName { get; }

        /// <summary>
        /// Adds a container with an initializer.
        /// </summary>
        /// <typeparam name="TInitializer">
        /// The <see cref="ICosmosContainerInitializer"/> to be
        /// used for initializing the container.
        /// </typeparam>
        /// <param name="name">The name of the container.</param>
        /// <param name="builder">The builder method, for configuring the Cosmos container.</param>
        /// <returns>The <see cref="ICosmosBuilder"/> instance.</returns>
        ICosmosBuilder AddContainer<TInitializer>(
            string name,
            Action<ICosmosContainerBuilder> builder)
            where TInitializer : class, ICosmosContainerInitializer;

        /// <summary>
        /// Adds a container.
        /// </summary>
        /// <param name="name">The name of the container.</param>
        /// <param name="builder">The builder method, for configuring the Cosmos container.</param>
        /// <returns>The <see cref="ICosmosBuilder"/> instance.</returns>
        ICosmosBuilder AddContainer(
            string name,
            Action<ICosmosContainerBuilder> builder);

        /// <summary>
        /// Adds a container with an initializer and a resource type.
        /// </summary>
        /// <typeparam name="TInitializer">
        /// The <see cref="ICosmosContainerInitializer"/> to be
        /// used for initializing the container.
        /// </typeparam>
        /// <typeparam name="TResource">
        /// The <see cref="ICosmosResource"/> to be used for
        /// representing document resources in the container.
        /// </typeparam>
        /// <param name="name">The name of the container.</param>
        /// <returns>The <see cref="ICosmosBuilder"/> instance.</returns>
        ICosmosBuilder<TResource> AddContainer<TInitializer, TResource>(
            string name)
            where TInitializer : class, ICosmosContainerInitializer
            where TResource : class, ICosmosResource;

        /// <summary>
        /// Adds a container with an initializer and a resource type.
        /// </summary>
        /// <typeparam name="TInitializer">
        /// The <see cref="ICosmosContainerInitializer"/> to be
        /// used for initializing the container.
        /// </typeparam>
        /// <param name="resourceType">
        /// The <see cref="ICosmosResource"/> to be used for
        /// representing document resources in the container.</param>
        /// <param name="name">The name of the container.</param>
        /// <returns>The <see cref="ICosmosBuilder"/> instance.</returns>
        ICosmosBuilder AddContainer<TInitializer>(
            Type resourceType,
            string name)
            where TInitializer : class, ICosmosContainerInitializer;

        /// <summary>
        /// Adds a container with a resource type.
        /// </summary>
        /// <typeparam name="TResource">
        /// The <see cref="ICosmosResource"/> to be used for
        /// representing document resources in the container.
        /// </typeparam>
        /// <param name="name">The name of the container.</param>
        /// <returns>The <see cref="ICosmosBuilder"/> instance.</returns>
        ICosmosBuilder<TResource> AddContainer<TResource>(
            string name)
            where TResource : class, ICosmosResource;

        /// <summary>
        /// Adds a container with a resource type.
        /// </summary>
        /// <param name="resourceType">
        /// The <see cref="ICosmosResource"/> to be used for
        /// representing document resources in the container.
        /// </param>
        /// <param name="name">The name of the container.</param>
        /// <returns>The <see cref="ICosmosBuilder"/> instance.</returns>
        ICosmosBuilder AddContainer(
            Type resourceType,
            string name);

        /// <summary>
        /// Configures a <see cref="IHostedService"/> for running the
        /// configured <see cref="ICosmosContainerInitializer"/>s.
        /// </summary>
        /// <remarks>
        /// This method should be called when the host is an AspNet service.
        /// For Azure Functions please use <see cref="ServiceCollectionExtensions.AzureFunctionInitializeCosmosDatabase(IServiceCollection)"/>.
        /// </remarks>
        /// <returns>The <see cref="ICosmosBuilder"/> instance.</returns>
        ICosmosBuilder UseHostedService();

        /// <summary>
        /// Creates a new <see cref="ICosmosBuilder"/> instance for the given database.
        /// </summary>
        /// <param name="databaseName">The database name for the builder to use.</param>
        /// <returns>A new builder instance for the given database.</returns>
        ICosmosBuilder ForDatabase(string databaseName);
    }
}