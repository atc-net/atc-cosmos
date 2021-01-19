using Microsoft.Extensions.DependencyInjection;

namespace Atc.Cosmos.Extensions
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
        /// Adds a <see cref="ICosmosResource"/> to be used for
        /// representing document resources in the container.
        /// </summary>
        /// <typeparam name="T">
        /// The <see cref="ICosmosResource"/> to be used for
        /// representing document resources in the container.
        /// </typeparam>
        /// <returns>An <see cref="ICosmosContainerBuilder" /> for configuring the container.</returns>
        ICosmosContainerBuilder AddResource<T>()
            where T : ICosmosResource;
    }
}