using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace Atc.Cosmos.Testing
{
    [SuppressMessage(
       "Design",
       "MA0016:Prefer return collection abstraction instead of implementation",
       Justification = "By design")]
    [SuppressMessage(
       "Design",
       "CA1002:Do not expose generic lists",
       Justification = "By design")]
    [SuppressMessage(
       "Usage",
       "CA2227:Collection properties should be read only",
       Justification = "By design")]
    public sealed class FakeCosmos<T> :
        ICosmosReader<T>,
        ICosmosWriter<T>,
        ICosmosBulkReader<T>,
        ICosmosBulkWriter<T>
        where T : class, ICosmosResource
    {
        public FakeCosmos()
            : this(
                  new FakeCosmosReader<T>(),
                  new FakeCosmosWriter<T>())
        {
        }

        public FakeCosmos(JsonSerializerOptions options)
            : this(
                  new FakeCosmosReader<T>(options),
                  new FakeCosmosWriter<T>(options))
        {
        }

        public FakeCosmos(
            FakeCosmosReader<T> reader,
            FakeCosmosWriter<T> writer)
        {
            Reader = reader;
            Writer = writer;
            writer.Documents = reader.Documents;
        }

        public List<T> Documents
        {
            get => Reader.Documents;
            set
            {
                Reader.Documents = value;
                Writer.Documents = value;
            }
        }

        public List<object> QueryResults
        {
            get => Reader.QueryResults;
            set
            {
                Reader.QueryResults = value;
            }
        }

        public FakeCosmosReader<T> Reader { get; }

        public FakeCosmosWriter<T> Writer { get; }

        Task<T?> ICosmosReader<T>.FindAsync(
            string documentId,
            string partitionKey,
            CancellationToken cancellationToken)
            => ((ICosmosReader<T>)Reader)
                .FindAsync(
                    documentId,
                    partitionKey,
                    cancellationToken);

        Task<T> ICosmosReader<T>.ReadAsync(
            string documentId,
            string partitionKey,
            CancellationToken cancellationToken)
            => ((ICosmosReader<T>)Reader)
                .ReadAsync(
                    documentId,
                    partitionKey,
                    cancellationToken);

        IAsyncEnumerable<T> ICosmosReader<T>.ReadAllAsync(
            string partitionKey,
            CancellationToken cancellationToken)
            => ((ICosmosReader<T>)Reader)
                .ReadAllAsync(
                    partitionKey,
                    cancellationToken);

        IAsyncEnumerable<T> ICosmosReader<T>.QueryAsync(
            QueryDefinition query,
            string partitionKey,
            CancellationToken cancellationToken)
            => ((ICosmosReader<T>)Reader)
                .QueryAsync(
                    query,
                    partitionKey,
                    cancellationToken);

        IAsyncEnumerable<TResult> ICosmosReader<T>.QueryAsync<TResult>(
            QueryDefinition query,
            string partitionKey,
            CancellationToken cancellationToken)
            => ((ICosmosReader<T>)Reader)
                .QueryAsync<TResult>(
                    query,
                    partitionKey,
                    cancellationToken);

        Task<PagedResult<T>> ICosmosReader<T>.PagedQueryAsync(
            QueryDefinition query,
            string partitionKey,
            int? pageSize,
            string? continuationToken,
            CancellationToken cancellationToken)
            => ((ICosmosReader<T>)Reader)
                .PagedQueryAsync(
                    query,
                    partitionKey,
                    pageSize,
                    continuationToken,
                    cancellationToken);

        Task<PagedResult<TResult>> ICosmosReader<T>.PagedQueryAsync<TResult>(
            QueryDefinition query,
            string partitionKey,
            int? pageSize,
            string? continuationToken,
            CancellationToken cancellationToken)
            => ((ICosmosReader<T>)Reader)
                .PagedQueryAsync<TResult>(
                    query,
                    partitionKey,
                    pageSize,
                    continuationToken,
                    cancellationToken);

        IAsyncEnumerable<T> ICosmosReader<T>.CrossPartitionQueryAsync(
            QueryDefinition query,
            CancellationToken cancellationToken)
            => ((ICosmosReader<T>)Reader)
                .CrossPartitionQueryAsync(
                    query,
                    cancellationToken);

        IAsyncEnumerable<TResult> ICosmosReader<T>.CrossPartitionQueryAsync<TResult>(
            QueryDefinition query,
            CancellationToken cancellationToken)
            => ((ICosmosReader<T>)Reader)
                .CrossPartitionQueryAsync<TResult>(
                    query,
                    cancellationToken);

        Task<PagedResult<T>> ICosmosReader<T>.CrossPartitionPagedQueryAsync(
            QueryDefinition query,
            int? pageSize,
            string? continuationToken,
            CancellationToken cancellationToken)
            => ((ICosmosReader<T>)Reader)
                .CrossPartitionPagedQueryAsync(
                    query,
                    pageSize,
                    continuationToken,
                    cancellationToken);

        Task<PagedResult<TResult>> ICosmosReader<T>.CrossPartitionPagedQueryAsync<TResult>(
            QueryDefinition query,
            int? pageSize,
            string? continuationToken,
            CancellationToken cancellationToken)
            => ((ICosmosReader<T>)Reader)
                .CrossPartitionPagedQueryAsync<TResult>(
                    query,
                    pageSize,
                    continuationToken,
                    cancellationToken);

        Task<T?> ICosmosBulkReader<T>.FindAsync(
            string documentId,
            string partitionKey,
            CancellationToken cancellationToken)
            => ((ICosmosBulkReader<T>)Reader)
                .FindAsync(
                    documentId,
                    partitionKey,
                    cancellationToken);

        Task<T> ICosmosBulkReader<T>.ReadAsync(
            string documentId,
            string partitionKey,
            CancellationToken cancellationToken)
            => ((ICosmosBulkReader<T>)Reader)
                .ReadAsync(
                    documentId,
                    partitionKey,
                    cancellationToken);

        IAsyncEnumerable<T> ICosmosBulkReader<T>.ReadAllAsync(
            string partitionKey,
            CancellationToken cancellationToken)
            => ((ICosmosBulkReader<T>)Reader)
                .ReadAllAsync(
                    partitionKey,
                    cancellationToken);

        IAsyncEnumerable<T> ICosmosBulkReader<T>.QueryAsync(
            QueryDefinition query,
            string partitionKey,
            CancellationToken cancellationToken)
            => ((ICosmosBulkReader<T>)Reader)
                .QueryAsync(
                    query,
                    partitionKey,
                    cancellationToken);

        IAsyncEnumerable<TResult> ICosmosBulkReader<T>.QueryAsync<TResult>(
            QueryDefinition query,
            string partitionKey,
            CancellationToken cancellationToken)
            => ((ICosmosBulkReader<T>)Reader)
                .QueryAsync<TResult>(
                    query,
                    partitionKey,
                    cancellationToken);

        IAsyncEnumerable<T> ICosmosBulkReader<T>.CrossPartitionQueryAsync(
            QueryDefinition query,
            CancellationToken cancellationToken)
            => ((ICosmosReader<T>)Reader)
                .CrossPartitionQueryAsync(
                    query,
                    cancellationToken);

        IAsyncEnumerable<TResult> ICosmosBulkReader<T>.CrossPartitionQueryAsync<TResult>(
            QueryDefinition query,
            CancellationToken cancellationToken)
            => ((ICosmosReader<T>)Reader)
                .CrossPartitionQueryAsync<TResult>(
                    query,
                    cancellationToken);

        Task<PagedResult<T>> ICosmosBulkReader<T>.CrossPartitionPagedQueryAsync(
            QueryDefinition query,
            int? pageSize,
            string? continuationToken,
            CancellationToken cancellationToken)
            => ((ICosmosReader<T>)Reader)
                .CrossPartitionPagedQueryAsync(
                    query,
                    pageSize,
                    continuationToken,
                    cancellationToken);

        Task<PagedResult<TResult>> ICosmosBulkReader<T>.CrossPartitionPagedQueryAsync<TResult>(
            QueryDefinition query,
            int? pageSize,
            string? continuationToken,
            CancellationToken cancellationToken)
            => ((ICosmosReader<T>)Reader)
                .CrossPartitionPagedQueryAsync<TResult>(
                    query,
                    pageSize,
                    continuationToken,
                    cancellationToken);

        Task<T> ICosmosWriter<T>.CreateAsync(
            T document,
            CancellationToken cancellationToken)
            => ((ICosmosWriter<T>)Writer)
                .CreateAsync(
                    document,
                    cancellationToken);

        Task ICosmosWriter<T>.CreateWithNoResponseAsync(
            T document,
            CancellationToken cancellationToken)
            => ((ICosmosWriter<T>)Writer)
                .CreateWithNoResponseAsync(
                    document,
                    cancellationToken);

        Task<T> ICosmosWriter<T>.WriteAsync(
            T document,
            CancellationToken cancellationToken)
            => ((ICosmosWriter<T>)Writer)
                .WriteAsync(
                    document,
                    cancellationToken);

        Task ICosmosWriter<T>.WriteWithNoResponseAsync(
            T document,
            CancellationToken cancellationToken)
            => ((ICosmosWriter<T>)Writer)
                .WriteWithNoResponseAsync(
                    document,
                    cancellationToken);

        Task<T> ICosmosWriter<T>.ReplaceAsync(
            T document,
            CancellationToken cancellationToken)
            => ((ICosmosWriter<T>)Writer)
                .ReplaceAsync(
                    document,
                    cancellationToken);

        Task ICosmosWriter<T>.ReplaceWithNoResponseAsync(
            T document,
            CancellationToken cancellationToken)
            => ((ICosmosWriter<T>)Writer)
                .ReplaceWithNoResponseAsync(
                    document,
                    cancellationToken);

        Task ICosmosWriter<T>.DeleteAsync(
            string documentId,
            string partitionKey,
            CancellationToken cancellationToken)
            => ((ICosmosWriter<T>)Writer)
                .DeleteAsync(
                    documentId,
                    partitionKey,
                    cancellationToken);

        Task<bool> ICosmosWriter<T>.TryDeleteAsync(
            string documentId,
            string partitionKey,
            CancellationToken cancellationToken)
            => ((ICosmosWriter<T>)Writer)
                .TryDeleteAsync(
                    documentId,
                    partitionKey,
                    cancellationToken);

        Task<T> ICosmosWriter<T>.UpdateAsync(
            string documentId,
            string partitionKey,
            Func<T, Task> updateDocument,
            int retries,
            CancellationToken cancellationToken)
            => ((ICosmosWriter<T>)Writer)
                .UpdateAsync(
                    documentId,
                    partitionKey,
                    updateDocument,
                    retries,
                    cancellationToken);

        Task<T> ICosmosWriter<T>.UpdateAsync(
            string documentId,
            string partitionKey,
            Action<T> updateDocument,
            int retries,
            CancellationToken cancellationToken)
            => ((ICosmosWriter<T>)Writer)
                .UpdateAsync(
                    documentId,
                    partitionKey,
                    updateDocument,
                    retries,
                    cancellationToken);

        Task<T> ICosmosWriter<T>.UpdateOrCreateAsync(
            Func<T> getDefaultDocument,
            Func<T, Task> updateDocument,
            int retries,
            CancellationToken cancellationToken)
            => ((ICosmosWriter<T>)Writer)
                .UpdateOrCreateAsync(
                    getDefaultDocument,
                    updateDocument,
                    retries,
                    cancellationToken);

        Task<T> ICosmosWriter<T>.UpdateOrCreateAsync(
            Func<T> getDefaultDocument,
            Action<T> updateDocument,
            int retries,
            CancellationToken cancellationToken)
            => ((ICosmosWriter<T>)Writer)
                .UpdateOrCreateAsync(
                    getDefaultDocument,
                    updateDocument,
                    retries,
                    cancellationToken);

        Task<T> ICosmosWriter<T>.PatchAsync(
            string documentId,
            string partitionKey,
            IReadOnlyList<PatchOperation> patchOperations,
            string? filterPredicate,
            CancellationToken cancellationToken)
            => ((ICosmosWriter<T>)Writer)
            .PatchAsync(
                documentId,
                partitionKey,
                patchOperations,
                filterPredicate,
                cancellationToken);

        Task ICosmosWriter<T>.PatchWithNoResponseAsync(
            string documentId,
            string partitionKey,
            IReadOnlyList<PatchOperation> patchOperations,
            string? filterPredicate,
            CancellationToken cancellationToken)
            => ((ICosmosWriter<T>)Writer)
                .PatchWithNoResponseAsync(
                    documentId,
                    partitionKey,
                    patchOperations,
                    filterPredicate,
                    cancellationToken);

        Task ICosmosBulkWriter<T>.CreateAsync(
             T document,
             CancellationToken cancellationToken)
             => ((ICosmosBulkWriter<T>)Writer)
                 .CreateAsync(
                     document,
                     cancellationToken);

        Task ICosmosBulkWriter<T>.WriteAsync(
            T document,
            CancellationToken cancellationToken)
            => ((ICosmosBulkWriter<T>)Writer)
                .WriteAsync(
                    document,
                    cancellationToken);

        Task ICosmosBulkWriter<T>.ReplaceAsync(
            T document,
            CancellationToken cancellationToken)
            => ((ICosmosBulkWriter<T>)Writer)
                .ReplaceAsync(
                    document,
                    cancellationToken);

        Task ICosmosBulkWriter<T>.DeleteAsync(
            string documentId,
            string partitionKey,
            CancellationToken cancellationToken)
            => ((ICosmosBulkWriter<T>)Writer)
                .DeleteAsync(
                    documentId,
                    partitionKey,
                    cancellationToken);
    }
}