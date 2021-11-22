using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Atc.Cosmos.Internal
{
    public class ChangeFeedService : IHostedService
    {
        private readonly IEnumerable<IChangeFeedListener> listeners;

        public ChangeFeedService(
            IEnumerable<IChangeFeedListener> listeners)
        {
            this.listeners = listeners;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var tasks = listeners
               .Select(l => l.StartAsync(cancellationToken))
               .ToArray();

            return Task.WhenAll(tasks);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            var tasks = listeners
               .Select(l => l.StopAsync(cancellationToken))
               .ToArray();

            return Task.WhenAll(tasks);
        }
    }
}
