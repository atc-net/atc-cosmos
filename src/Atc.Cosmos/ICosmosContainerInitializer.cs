using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace Atc.Cosmos
{
    /// <summary>
    /// Represents an initializer for a Cosmos container.
    /// </summary>
    public interface ICosmosContainerInitializer
    {
        /// <summary>
        /// Initializes the container in Cosmos.
        /// </summary>
        /// <param name="database">The Cosmos database.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task InitializeAsync(
            Database database,
            CancellationToken cancellationToken);
    }
}