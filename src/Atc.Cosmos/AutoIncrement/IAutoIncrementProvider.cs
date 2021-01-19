using System.Threading;
using System.Threading.Tasks;

namespace Atc.Cosmos
{
    /// <summary>
    /// Represents a provider for auto-incrementing integers.
    /// </summary>
    public interface IAutoIncrementProvider
    {
        /// <summary>
        /// Returns the next integer from the specified counter.
        /// </summary>
        /// <param name="counterName">The unique name of the counter.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used.</param>
        /// <returns>The next integer of the specified counter, starting with 1.</returns>
        Task<int> GetNextAsync(
            string counterName,
            CancellationToken cancellationToken);
    }
}