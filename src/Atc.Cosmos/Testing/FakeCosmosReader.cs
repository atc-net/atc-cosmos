using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Atc.Cosmos.Internal;
using Microsoft.Azure.Cosmos;
using static System.FormattableString;

namespace Atc.Cosmos.Testing
{
    /// <summary>
    /// Represents a fake <see cref="ICosmosReader{T}"/>
    /// or <see cref="ICosmosBulkReader{T}"/> that can be
    /// used when unit testing client code.
    /// </summary>
    /// <typeparam name="T">
    /// The type of <see cref="ICosmosResource"/>
    /// to be read by this reader.
    /// </typeparam>
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
    public class FakeCosmosReader<T> :
        ICosmosReader<T>,
        ICosmosBulkReader<T>
        where T : class, ICosmosResource
    {
        private readonly JsonSerializerOptions? options;

        public FakeCosmosReader()
        {
        }

        public FakeCosmosReader(JsonSerializerOptions options)
        {
            this.options = options;
        }

        /// <summary>
        /// Gets or sets the list of documents to return by the fake reader.
        /// </summary>
        public List<T> Documents { get; set; }
            = new List<T>();

        /// <summary>
        /// Gets or sets the list of custom results to be returned by the
        /// <see cref="QueryAsync{TResult}(QueryDefinition, string, CancellationToken)"/> method.
        /// </summary>
        public List<object> QueryResults { get; set; }
            = new List<object>();

        public virtual Task<T?> FindAsync(
            string documentId,
            string partitionKey,
            CancellationToken cancellationToken = default)
            => Task.FromResult<T?>(
                Documents
                    .Find(d
                        => d.DocumentId == documentId
                        && d.PartitionKey == partitionKey)
                    .Clone(options));

        public virtual Task<T> ReadAsync(
            string documentId,
            string partitionKey,
            CancellationToken cancellationToken = default)
        {
            var item = Documents.Find(d
               => d.DocumentId == documentId
               && d.PartitionKey == partitionKey);

            if (item is null)
            {
                throw new CosmosException(
                    $"Document not found. " +
                    $"Id: {documentId}. " +
                    $"PartitionKey: {partitionKey}",
                    HttpStatusCode.NotFound,
                    0,
                    string.Empty,
                    0);
            }

            return Task.FromResult(item.Clone(options));
        }

        public virtual IAsyncEnumerable<T> ReadAllAsync(
            string partitionKey,
            CancellationToken cancellationToken = default)
            => GetAsyncEnumerator(Documents
                .Where(d => d.PartitionKey == partitionKey)
                .Clone(options));

        public virtual IAsyncEnumerable<T> QueryAsync(
            QueryDefinition query,
            string partitionKey,
            CancellationToken cancellationToken = default)
            => QueryAsync<T>(
                query,
                partitionKey,
                cancellationToken);

        public virtual IAsyncEnumerable<TResult> QueryAsync<TResult>(
            QueryDefinition query,
            string partitionKey,
            CancellationToken cancellationToken = default)
            => GetAsyncEnumerator(QueryResults
                .OfType<TResult>()
                .Clone(options));

        public virtual IAsyncEnumerable<TResult> QueryAsync<TResult>(
            Func<IQueryable<T>, IQueryable<TResult>> queryBuilder,
            string partitionKey,
            CancellationToken cancellationToken = default)
            => GetAsyncEnumerator(
                queryBuilder(
                    Documents
                        .Where(d => d.PartitionKey == partitionKey)
                        .AsQueryable()));

        public virtual Task<PagedResult<T>> PagedQueryAsync(
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

        public virtual Task<PagedResult<TResult>> PagedQueryAsync<TResult>(
            QueryDefinition query,
            string partitionKey,
            int? pageSize,
            string? continuationToken = default,
            CancellationToken cancellationToken = default)
        {
            var startIndex = GetStartIndex(continuationToken);
            var items = QueryResults
                .OfType<TResult>()
                .Skip(startIndex)
                .Take(pageSize ?? 3)
                .Select(o => o.Clone(options))
                .ToList();

            return Task.FromResult(new PagedResult<TResult>
            {
                Items = items,
                ContinuationToken = GetContinuationToken(startIndex, items.Count),
            });
        }

        public virtual Task<PagedResult<TResult>> PagedQueryAsync<TResult>(
            Func<IQueryable<T>, IQueryable<TResult>> queryBuilder,
            string partitionKey,
            int? pageSize,
            string? continuationToken = default,
            CancellationToken cancellationToken = default)
        {
            var startIndex = GetStartIndex(continuationToken);
            var items =
                queryBuilder(
                        Documents
                            .Where(d => d.PartitionKey == partitionKey)
                            .AsQueryable())
                    .Skip(startIndex)
                    .Take(pageSize ?? 3)
                    .ToList();

            return Task.FromResult(new PagedResult<TResult>
            {
                Items = items,
                ContinuationToken = GetContinuationToken(startIndex, items.Count),
            });
        }

        public virtual IAsyncEnumerable<T> CrossPartitionQueryAsync(
            QueryDefinition query,
            CancellationToken cancellationToken = default)
            => CrossPartitionQueryAsync<T>(
                query,
                cancellationToken);

        public virtual IAsyncEnumerable<TResult> CrossPartitionQueryAsync<TResult>(
            QueryDefinition query,
            CancellationToken cancellationToken = default)
            => GetAsyncEnumerator(QueryResults
                .OfType<TResult>()
                .Clone(options));

        public virtual IAsyncEnumerable<TResult> CrossPartitionQueryAsync<TResult>(
            Func<IQueryable<T>, IQueryable<TResult>> queryBuilder,
            CancellationToken cancellationToken = default)
            => GetAsyncEnumerator(queryBuilder(Documents.AsQueryable()));

        public virtual Task<PagedResult<T>> CrossPartitionPagedQueryAsync(
            QueryDefinition query,
            int? pageSize,
            string? continuationToken = default,
            CancellationToken cancellationToken = default)
            => CrossPartitionPagedQueryAsync<T>(
                query,
                pageSize,
                continuationToken,
                cancellationToken);

        public virtual Task<PagedResult<TResult>> CrossPartitionPagedQueryAsync<TResult>(
            QueryDefinition query,
            int? pageSize,
            string? continuationToken = default,
            CancellationToken cancellationToken = default)
        {
            var startIndex = GetStartIndex(continuationToken);
            var items = QueryResults
                .OfType<TResult>()
                .Skip(startIndex)
                .Take(pageSize ?? 3)
                .Select(o => o.Clone(options))
                .ToList();

            return Task.FromResult(new PagedResult<TResult>
            {
                Items = items,
                ContinuationToken = GetContinuationToken(startIndex, items.Count),
            });
        }

        public virtual Task<PagedResult<TResult>> CrossPartitionPagedQueryAsync<TResult>(
            Func<IQueryable<T>, IQueryable<TResult>> queryBuilder,
            int? pageSize,
            string? continuationToken = default,
            CancellationToken cancellationToken = default)
        {
            var startIndex = GetStartIndex(continuationToken);
            var items = queryBuilder(Documents.AsQueryable())
                .Skip(startIndex)
                .Take(pageSize ?? 3)
                .ToList();

            return Task.FromResult(new PagedResult<TResult>
            {
                Items = items, ContinuationToken = GetContinuationToken(startIndex, items.Count),
            });
        }

        public IAsyncEnumerable<IEnumerable<T>> BatchReadAllAsync(
            string partitionKey,
            CancellationToken cancellationToken = default)
            => GetAsyncEnumerator(Documents
                .Where(d => d.PartitionKey == partitionKey)
                .Chunk(3));

        public IAsyncEnumerable<IEnumerable<T>> BatchQueryAsync(
            QueryDefinition query,
            string partitionKey,
            CancellationToken cancellationToken = default)
            => GetAsyncEnumerator(QueryResults
                .OfType<T>()
                .Chunk(3));

        public IAsyncEnumerable<IEnumerable<TResult>> BatchQueryAsync<TResult>(
            QueryDefinition query,
            string partitionKey,
            CancellationToken cancellationToken = default)
            => GetAsyncEnumerator(QueryResults
                .OfType<TResult>()
                .Chunk(3));

        public virtual async IAsyncEnumerable<IEnumerable<TResult>> BatchQueryAsync<TResult>(
            Func<IQueryable<T>, IQueryable<TResult>> queryBuilder,
            string partitionKey,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var queryResults = QueryAsync(queryBuilder, partitionKey, cancellationToken);
            var buffer = new List<TResult>();
            await foreach (var result in queryResults.WithCancellation(cancellationToken))
            {
                buffer.Add(result);
                if (buffer.Count == 3)
                {
                    yield return buffer.ToImmutableList();
                    buffer.Clear();
                }
            }

            if (buffer.Count > 0)
            {
                yield return buffer.ToImmutableList();
            }
        }

        public IAsyncEnumerable<IEnumerable<T>> BatchCrossPartitionQueryAsync(
            QueryDefinition query,
            CancellationToken cancellationToken = default)
            => GetAsyncEnumerator(QueryResults
                .OfType<T>()
                .Chunk(3));

        public IAsyncEnumerable<IEnumerable<TResult>> BatchCrossPartitionQueryAsync<TResult>(
            QueryDefinition query,
            CancellationToken cancellationToken = default)
            => GetAsyncEnumerator(QueryResults
                .OfType<TResult>()
                .Chunk(3));

        public virtual async IAsyncEnumerable<IEnumerable<TResult>> BatchCrossPartitionQueryAsync<TResult>(
            Func<IQueryable<T>, IQueryable<TResult>> queryBuilder,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var queryResults = CrossPartitionQueryAsync(queryBuilder, cancellationToken);
            var buffer = new List<TResult>();
            await foreach (var result in queryResults.WithCancellation(cancellationToken))
            {
                buffer.Add(result);
                if (buffer.Count == 3)
                {
                    yield return buffer.ToImmutableList();
                    buffer.Clear();
                }
            }

            if (buffer.Count > 0)
            {
                yield return buffer.ToImmutableList();
            }
        }

        protected static int GetStartIndex(string? continuationToken)
            => continuationToken is not null
            && int.TryParse(
                continuationToken,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var index)
            ? index
            : 0;

        protected static string? GetContinuationToken(
            int startIndex,
            int itemsCount)
            => itemsCount > 0
             ? Invariant($"{startIndex + itemsCount}")
             : null;

        protected static async IAsyncEnumerable<TItem> GetAsyncEnumerator<TItem>(
            IEnumerable<TItem> items)
        {
            foreach (var item in items)
            {
                yield return await Task
                    .FromResult(item)
                    .ConfigureAwait(false);
            }
        }
    }
}