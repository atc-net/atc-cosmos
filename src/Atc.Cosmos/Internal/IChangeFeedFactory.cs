using Microsoft.Azure.Cosmos;

namespace Atc.Cosmos.Internal
{
    public interface IChangeFeedFactory
    {
        ChangeFeedProcessor Create<T>(
            Container.ChangesHandler<T> onChanges,
            Container.ChangeFeedMonitorErrorDelegate? onError = null,
            string? processorName = null);
    }
}