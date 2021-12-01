using System.Threading;
using System.Threading.Tasks;

namespace Atc.Cosmos.Internal
{
    /// <summary>
    /// Represents the initializer responsible for executing
    /// the configured <see cref="ICosmosContainerInitializer"/>s.
    /// </summary>
    public interface ICosmosInitializer
    {
        /// <summary>
        /// Run the initialization that will execute the
        /// configured <see cref="ICosmosContainerInitializer"/>s.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task InitializeAsync(CancellationToken cancellationToken);
    }
}