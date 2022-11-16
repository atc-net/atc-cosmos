using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace Atc.Cosmos.Internal
{
    public class CosmosReader<T> : ICosmosReader<T>
        where T : class, ICosmosResource
    {
        private const string ReadAllQuery = "SELECT * FROM c";
        private readonly Container container;
        private readonly IOptions<CosmosOptions> options;

        public CosmosReader(
            ICosmosContainerProvider containerProvider,
            IOptions<CosmosOptions> options)
        {
            this.container = containerProvider.GetContainer<T>();
            this.options = options;
        }

        public async Task<T> ReadAsync(
            string documentId,
            string partitionKey,
            CancellationToken cancellationToken = default)
        {
            var result = await container
                .ReadItemAsync<T>(
                    documentId,
                    new PartitionKey(partitionKey),
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            result.Resource.ETag = result.ETag;

            return result.Resource;
        }

        public async Task<T?> FindAsync(
            string documentId,
            string partitionKey,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await ReadAsync(
                    documentId,
                    partitionKey,
                    cancellationToken).ConfigureAwait(false);
            }
            catch (CosmosException)
            {
                return default;
            }
        }

        public async IAsyncEnumerable<T> ReadAllAsync(
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
                foreach (var document in documents)
                {
                    yield return document;
                }
            }
        }

        public IAsyncEnumerable<T> QueryAsync(
            QueryDefinition query,
            string partitionKey,
            CancellationToken cancellationToken = default)
            => QueryAsync<T>(query, partitionKey, cancellationToken);

        public async IAsyncEnumerable<TResult> QueryAsync<TResult>(
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
                foreach (var document in documents)
                {
                    yield return document;
                }
            }
        }

        public Task<PagedResult<T>> PagedQueryAsync(
            QueryDefinition query,
            string partitionKey,
            int? pageSize,
            string? continuationToken = default,
            CancellationToken cancellationToken = default)
            => PagedQueryAsync<T>(
                query,
                partitionKey,
                pageSize,
                continuationToken,
                cancellationToken);

        public async Task<PagedResult<TResult>> PagedQueryAsync<TResult>(
            QueryDefinition query,
            string partitionKey,
            int? pageSize,
            string? continuationToken = default,
            CancellationToken cancellationToken = default)
        {
            var reader = container.GetItemQueryIterator<TResult>(
                query,
                continuationToken,
                requestOptions: new QueryRequestOptions
                {
                    PartitionKey = new PartitionKey(partitionKey),
                    MaxItemCount = pageSize,
                    ResponseContinuationTokenLimitInKb = options.Value.ContinuationTokenLimitInKb,
                });

            if (!reader.HasMoreResults)
            {
                return new PagedResult<TResult>();
            }

            var result = await reader
                .ReadNextAsync(cancellationToken)
                .ConfigureAwait(false);

            return new PagedResult<TResult>
            {
                Items = result.ToArray(),
                ContinuationToken = result.ContinuationToken,
            };
        }

        public IAsyncEnumerable<T> CrossPartitionQueryAsync(
            QueryDefinition query,
            CancellationToken cancellationToken = default)
            => CrossPartitionQueryAsync<T>(query, cancellationToken);

        public async IAsyncEnumerable<TResult> CrossPartitionQueryAsync<TResult>(
            QueryDefinition query,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var reader = container.GetItemQueryIterator<TResult>(query);

            while (reader.HasMoreResults && !cancellationToken.IsCancellationRequested)
            {
                var documents = await reader
                    .ReadNextAsync(cancellationToken)
                    .ConfigureAwait(false);
                foreach (var document in documents)
                {
                    yield return document;
                }
            }
        }

        public Task<PagedResult<T>> CrossPartitionPagedQueryAsync(
            QueryDefinition query,
            int? pageSize,
            string? continuationToken = default,
            CancellationToken cancellationToken = default)
            => CrossPartitionPagedQueryAsync<T>(query, pageSize, continuationToken, cancellationToken);

        public async Task<PagedResult<TResult>> CrossPartitionPagedQueryAsync<TResult>(
            QueryDefinition query,
            int? pageSize,
            string? continuationToken = default,
            CancellationToken cancellationToken = default)
        {
            var reader = container.GetItemQueryIterator<TResult>(
                query,
                continuationToken,
                requestOptions: new QueryRequestOptions
                {
                    MaxItemCount = pageSize,
                    ResponseContinuationTokenLimitInKb = options.Value.ContinuationTokenLimitInKb,
                });

            if (!reader.HasMoreResults)
            {
                return new PagedResult<TResult>();
            }

            var result = await reader
                .ReadNextAsync(cancellationToken)
                .ConfigureAwait(false);

            return new PagedResult<TResult>
            {
                Items = result.ToArray(),
                ContinuationToken = result.ContinuationToken,
            };
        }
    }
}