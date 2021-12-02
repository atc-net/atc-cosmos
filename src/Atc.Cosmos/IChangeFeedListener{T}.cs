using Atc.Cosmos.Internal;

namespace Atc.Cosmos
{
    /// <summary>
    /// Represents a change feed listener for a specific <see cref="ICosmosResource"/>
    /// and can be used to start and stop the listening for changes.
    /// </summary>
    /// <typeparam name="T">The <see cref="ICosmosResource"/>.</typeparam>
    public interface IChangeFeedListener<T> : IChangeFeedListener
        where T : class, ICosmosResource
    {
    }
}
