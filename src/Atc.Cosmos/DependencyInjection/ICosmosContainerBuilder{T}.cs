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
        ICosmosContainerBuilder<T> WithChangeFeedProcessor<TProcessor>()
            where TProcessor : class, IChangeFeedProcessor<T>;
    }
}