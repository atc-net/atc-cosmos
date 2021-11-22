using System.Threading;
using System.Threading.Tasks;

namespace Atc.Cosmos.Internal
{
    public interface IChangeFeedListener
    {
        Task StartAsync(CancellationToken cancellationToken);

        Task StopAsync(CancellationToken cancellationToken);
    }
}