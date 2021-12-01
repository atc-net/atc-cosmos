using System.Threading;
using System.Threading.Tasks;

namespace Atc.Cosmos.Internal
{
    /// <summary>
    /// Represents a change feed listener and can be used
    /// to start and stop the listening for changes.
    /// </summary>
    public interface IChangeFeedListener
    {
        /// <summary>
        /// Start listening for changes.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task StartAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Stop listening for changes.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task StopAsync(CancellationToken cancellationToken);
    }
}