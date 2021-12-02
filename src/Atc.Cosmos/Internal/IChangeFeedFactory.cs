using Microsoft.Azure.Cosmos;

namespace Atc.Cosmos.Internal
{
    /// <summary>
    /// Represents a factory for creating a <see cref="ChangeFeedProcessor"/>
    /// for a <see cref="ICosmosResource"/>.
    /// </summary>
    public interface IChangeFeedFactory
    {
        /// <summary>
        /// Create a <see cref="ChangeFeedProcessor"/>.
        /// </summary>
        /// <typeparam name="T">The <see cref="ICosmosResource"/>.</typeparam>
        /// <param name="onChanges">Delegate to receive changes.</param>
        /// <param name="onError">A delegate to receive notifications for change feed processor related errors.</param>
        /// <param name="processorName">A name that identifies the Processor and the particular work it will do.</param>
        /// <returns>A <see cref="ChangeFeedProcessor"/>.</returns>
        ChangeFeedProcessor Create<T>(
            Container.ChangesHandler<T> onChanges,
            Container.ChangeFeedMonitorErrorDelegate? onError = null,
            string? processorName = null);
    }
}