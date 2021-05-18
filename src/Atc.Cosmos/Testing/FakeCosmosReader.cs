using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

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
                Documents.Find(d
                    => d.DocumentId == documentId
                    && d.PartitionKey == partitionKey));

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

            return Task.FromResult(item);
        }

        public virtual IAsyncEnumerable<T> ReadAllAsync(
            string partitionKey,
            CancellationToken cancellationToken = default)
            => GetAsyncEnumerator(
                Documents.Where(d => d.PartitionKey == partitionKey));

        public virtual IAsyncEnumerable<T> QueryAsync(
            QueryDefinition query,
            string partitionKey,
            CancellationToken cancellationToken = default)
            => GetAsyncEnumerator(
                Documents.Where(d => d.PartitionKey == partitionKey));

        public virtual IAsyncEnumerable<TResult> QueryAsync<TResult>(
            QueryDefinition query,
            string partitionKey,
            CancellationToken cancellationToken = default)
            => GetAsyncEnumerator(
                QueryResults.OfType<TResult>());

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