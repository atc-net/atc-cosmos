using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace Atc.Cosmos.Internal
{
    public class PartitionedChangeFeedListener<TResource, TProcessor> : IChangeFeedListener
        where TResource : class, ICosmosResource
        where TProcessor : IChangeFeedProcessor<TResource>
    {
        private readonly ChangeFeedProcessor changeFeed;
        private readonly TProcessor processor;

        public PartitionedChangeFeedListener(
            IChangeFeedFactory changeFeedFactory,
            TProcessor processor)
        {
            this.changeFeed = changeFeedFactory
                .Create<TResource>(
                OnChanges,
                processor.ErrorAsync);
            this.processor = processor;
        }

        public Task StartAsync(CancellationToken cancellationToken)
            => changeFeed.StartAsync();

        public Task StopAsync(CancellationToken cancellationToken)
            => changeFeed.StopAsync();

        private Task OnChanges(
            IReadOnlyCollection<TResource> changes,
            CancellationToken cancellationToken)
        {
            var partitions = changes.GroupBy(c => c.PartitionKey, StringComparer.Ordinal);
            var tasks = partitions
                .Select(g => processor.ProcessAsync(
                    g.Key,
                    g.ToArray(),
                    cancellationToken))
                .ToArray();

            return Task.WhenAll(tasks);
        }
    }
}
