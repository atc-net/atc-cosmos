using Microsoft.Extensions.DependencyInjection;

namespace Atc.Cosmos.Extensions
{
    /// <summary>
    /// Represents a builder for configuring a Cosmos container.
    /// </summary>
    public interface ICosmosContainerBuilder
    {
        /// <summary>
        /// The service collection.
        /// </summary>
        public IServiceCollection Services { get; }

        /// <summary>
        /// The name of the container being built.
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
        ICosmosContainerBuilder AddResource<T>()
            where T : ICosmosResource;
    }
}