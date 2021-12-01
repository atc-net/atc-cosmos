using Microsoft.Extensions.DependencyInjection;

namespace Atc.Cosmos.DependencyInjection
{
    /// <summary>
    /// Represents a builder for configuring a Cosmos container.
    /// </summary>
    /// <typeparam name="T">
    /// The type of <see cref="ICosmosResource"/>
    /// used for further build operations.
    /// </typeparam>
    public interface ICosmosContainerBuilder<T> : ICosmosContainerBuilder
        where T : class, ICosmosResource
    {
        /// <summary>
        /// Adds a <see cref="IChangeFeedProcessor{T}"/> to the container.
        /// </summary>
        /// <typeparam name="TProcessor">The <see cref="IChangeFeedProcessor{T}"/> type.</typeparam>
        /// <param name="maxDegreeOfParallelism">The maximum parallel calls to the <see cref="IChangeFeedProcessor{T}"/>.</param>
        /// <returns>The <see cref="ICosmosContainerBuilder{T}"/> instance.</returns>
        ICosmosContainerBuilder<T> WithChangeFeedProcessor<TProcessor>(
            int maxDegreeOfParallelism = 1)
            where TProcessor : class, IChangeFeedProcessor<T>;
    }
}