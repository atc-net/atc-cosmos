using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace Atc.Cosmos.Internal
{
    public class ChangeFeedListener<TResource, TProcessor> : IChangeFeedListener<TResource>
        where TResource : class, ICosmosResource
        where TProcessor : IChangeFeedProcessor<TResource>
    {
        private readonly ChangeFeedProcessor changeFeed;
        private readonly TProcessor processor;
        private readonly int maxDegreeOfParallelism;

        public ChangeFeedListener(
            IChangeFeedFactory changeFeedFactory,
            TProcessor processor,
            int maxDegreeOfParallelism)
        {
            this.changeFeed = changeFeedFactory
                .Create<TResource>(
                OnChanges,
                processor.ErrorAsync);
            this.processor = processor;
            this.maxDegreeOfParallelism = maxDegreeOfParallelism;
        }

        public Task StartAsync(CancellationToken cancellationToken)
            => changeFeed.StartAsync();

        public Task StopAsync(CancellationToken cancellationToken)
            => changeFeed.StopAsync();

        private async Task OnChanges(
            IReadOnlyCollection<TResource> changes,
            CancellationToken cancellationToken)
        {
            var partitions = changes
                .GroupBy(c => c.PartitionKey, StringComparer.Ordinal);

            var batches = partitions
                .Chunk(maxDegreeOfParallelism);

            foreach (var batch in batches)
            {
                var tasks = batch
                   .Select(g => processor.ProcessAsync(
                       g.Key,
                       g.ToArray(),
                       cancellationToken))
                   .ToArray();

                await Task.WhenAll(tasks)
                    .ConfigureAwait(false);
            }
        }
    }
}
