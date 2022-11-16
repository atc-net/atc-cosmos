using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace Atc.Cosmos.Internal
{
    public class CosmosBatchReader<T> : ICosmosBatchReader<T>
        where T : class, ICosmosResource
    {
        private const string ReadAllQuery = "SELECT * FROM c";
        private readonly Container container;
        private readonly IOptions<CosmosOptions> options;

        public CosmosBatchReader(
            ICosmosContainerProvider containerProvider,
            IOptions<CosmosOptions> options)
        {
            this.container = containerProvider.GetContainer<T>();
            this.options = options;
        }

        public async IAsyncEnumerable<IEnumerable<T>> ReadAllAsync(
            string partitionKey,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var reader = container.GetItemQueryIterator<T>(
                ReadAllQuery,
                requestOptions: new QueryRequestOptions
                {
                    PartitionKey = new PartitionKey(partitionKey),
                });

            while (reader.HasMoreResults && !cancellationToken.IsCancellationRequested)
            {
                var documents = await reader
                    .ReadNextAsync(cancellationToken)
                    .ConfigureAwait(false);

                yield return documents;
            }
        }

        public IAsyncEnumerable<IEnumerable<T>> QueryAsync(
            QueryDefinition query,
            string partitionKey,
            CancellationToken cancellationToken = default)
            => QueryAsync<T>(query, partitionKey, cancellationToken);

        public async IAsyncEnumerable<IEnumerable<TResult>> QueryAsync<TResult>(
            QueryDefinition query,
            string partitionKey,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var reader = container.GetItemQueryIterator<TResult>(
                query,
                requestOptions: new QueryRequestOptions
                {
                    PartitionKey = new PartitionKey(partitionKey),
                });

            while (reader.HasMoreResults && !cancellationToken.IsCancellationRequested)
            {
                var documents = await reader
                    .ReadNextAsync(cancellationToken)
                    .ConfigureAwait(false);

                yield return documents;
            }
        }

        public IAsyncEnumerable<IEnumerable<T>> CrossPartitionQueryAsync(
            QueryDefinition query,
            CancellationToken cancellationToken = default)
            => CrossPartitionQueryAsync<T>(query, cancellationToken);

        public async IAsyncEnumerable<IEnumerable<TResult>> CrossPartitionQueryAsync<TResult>(
            QueryDefinition query,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var reader = container.GetItemQueryIterator<TResult>(query);

            while (reader.HasMoreResults && !cancellationToken.IsCancellationRequested)
            {
                var documents = await reader
                    .ReadNextAsync(cancellationToken)
                    .ConfigureAwait(false);

                yield return documents;
            }
        }
    }
}