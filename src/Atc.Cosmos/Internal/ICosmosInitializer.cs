using System.Threading;
using System.Threading.Tasks;

namespace Atc.Cosmos.Internal
{
    public interface ICosmosInitializer
    {
        Task InitializeAsync(CancellationToken cancellationToken);
    }
}