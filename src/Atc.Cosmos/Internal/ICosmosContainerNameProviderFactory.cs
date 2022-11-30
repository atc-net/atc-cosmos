using System;

namespace Atc.Cosmos.Internal
{
    /// <summary>
    /// Represents the factory for <see cref="ICosmosContainerNameProvider"/> instances.
    /// </summary>
    public interface ICosmosContainerNameProviderFactory
    {
        /// <summary>
        /// Register new provider for the type of <see cref="ICosmosResource"/> with the given container name and options.
        /// Options are optional and will apply default options if null.
        /// If the <see cref="ICosmosResource"/> type has already been registered an <see cref="NotSupportedException"/> will be thrown.
        /// </summary>
        /// <typeparam name="T">The <see cref="ICosmosResource"/> to tie to container name.</typeparam>
        /// <param name="containerName">The container name to tie this resource.</param>
        /// <param name="options">Optional <see cref="CosmosOptions"/> to associate with container.</param>
        /// <returns>The newly created <see cref="ICosmosContainerNameProvider"/> instance.</returns>
        ICosmosContainerNameProvider Register<T>(string containerName, CosmosOptions? options = default)
            where T : ICosmosResource;

        /// <summary>
        /// Register new provider for the type of <see cref="ICosmosResource"/> with the given container name and options.
        /// Options are optional and will apply default options if null.
        /// If the <see cref="ICosmosResource"/> type has already been registered an <see cref="NotSupportedException"/> will be thrown.
        /// </summary>
        /// <param name="resourceType">The <see cref="ICosmosResource"/> to tie to container name.</param>
        /// <param name="containerName">The container name to tie this resource.</param>
        /// <param name="options">Optional <see cref="CosmosOptions"/> to associate with container.</param>
        /// <returns>The newly created <see cref="ICosmosContainerNameProvider"/> instance.</returns>
        ICosmosContainerNameProvider Register(Type resourceType, string containerName, CosmosOptions? options = default);
    }
}