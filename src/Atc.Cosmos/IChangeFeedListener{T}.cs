using System.Diagnostics.CodeAnalysis;
using Atc.Cosmos.Internal;

namespace Atc.Cosmos
{
    /// <summary>
    /// Represents a change feed listener for a specific <see cref="ICosmosResource"/>
    /// and can be used to start and stop the listening for changes.
    /// </summary>
    /// <typeparam name="T">The <see cref="ICosmosResource"/>.</typeparam>
    [SuppressMessage(
        "Major Code Smell",
        "S2326:Unused type parameters should be removed",
        Justification = "By design. Interface is used for resolving specific IChangeFeedListener via DI")]
    public interface IChangeFeedListener<T> : IChangeFeedListener
        where T : class, ICosmosResource
    {
    }
}
