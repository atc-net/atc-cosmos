using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Atc.Cosmos.Internal
{
    /// <summary>
    /// Responsible for initializing cosmos database and containers doing aspnet core startup
    /// before the API is serving requests.
    /// </summary>
    public class StartupInitializationJob : IHostedService
    {
        private readonly ICosmosInitializer initializer;

        public StartupInitializationJob(ICosmosInitializer initializer)
        {
            this.initializer = initializer;
        }

        public Task StartAsync(CancellationToken cancellationToken)
            => initializer.InitializeAsync(cancellationToken);

        public Task StopAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;
    }
}