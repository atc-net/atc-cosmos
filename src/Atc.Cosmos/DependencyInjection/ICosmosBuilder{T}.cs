namespace Atc.Cosmos.DependencyInjection
{
    /// <summary>
    /// Represents a builder for configuring Cosmos.
    /// </summary>
    /// <typeparam name="T">
    /// The type of <see cref="ICosmosResource"/>
    /// used for further build operations.
    /// </typeparam>
    public interface ICosmosBuilder<T> : ICosmosBuilder
        where T : class, ICosmosResource
    {
        ICosmosBuilder<T> WithChangeFeedProcessor<TProcessor>()
            where TProcessor : class, IChangeFeedProcessor<T>;
    }
}
