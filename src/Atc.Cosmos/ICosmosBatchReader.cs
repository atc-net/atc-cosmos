using System.Collections.Generic;
using System.Threading;
using Microsoft.Azure.Cosmos;

namespace Atc.Cosmos
{
    /// <summary>
    /// Represents a reader that can read Cosmos resources in slices.
    /// </summary>
    /// <typeparam name="T">
    /// The type of <see cref="ICosmosResource"/>
    /// to be read by this reader.
    /// </typeparam>
    public interface ICosmosBatchReader<out T>
        where T : class, ICosmosResource
    {
        /// <summary>
        /// Reads all the specified <typeparamref name="T"/> resource from the configured
        /// Cosmos collection.
        /// </summary>
        /// <param name="partitionKey">Partition key of the resource.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used.</param>
        /// <returns>An <see cref="IAsyncEnumerable&lt;T&gt;"/> over all the <typeparamref name="T"/> resources.</returns>
        public IAsyncEnumerable<IEnumerable<T>> ReadAllAsync(
            string partitionKey,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Query documents from the configured Cosmos container.
        /// </summary>
        /// <param name="query">Cosmos query to execute.</param>
        /// <param name="partitionKey">Partition key of the resource.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used.</param>
        /// <returns>An <see cref="IAsyncEnumerable&lt;T&gt;"/> over the requested <typeparamref name="T"/> resources.</returns>
        public IAsyncEnumerable<IEnumerable<T>> QueryAsync(
            QueryDefinition query,
            string partitionKey,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Query documents from the configured Cosmos container and returns a custom result.
        /// </summary>
        /// <typeparam name="TResult">The type used for the custom query result.</typeparam>
        /// <param name="query">Cosmos query to execute.</param>
        /// <param name="partitionKey">Partition key of the resource.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used.</param>
        /// <returns>An <see cref="IAsyncEnumerable&lt;T&gt;"/> over the requested <typeparamref name="TResult"/> resources.</returns>
        public IAsyncEnumerable<IEnumerable<TResult>> QueryAsync<TResult>(
            QueryDefinition query,
            string partitionKey,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Query documents across partitions from the configured Cosmos container.
        /// </summary>
        /// <param name="query">Cosmos query to execute.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used.</param>
        /// <returns>An <see cref="IAsyncEnumerable&lt;T&gt;"/> over the requested <typeparamref name="T"/> resources.</returns>
        public IAsyncEnumerable<IEnumerable<T>> CrossPartitionQueryAsync(
            QueryDefinition query,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Query documents across partitions from the configured Cosmos container and returns a custom result.
        /// </summary>
        /// <typeparam name="TResult">The type used for the custom query result.</typeparam>
        /// <param name="query">Cosmos query to execute.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used.</param>
        /// <returns>An <see cref="IAsyncEnumerable&lt;T&gt;"/> over the requested <typeparamref name="TResult"/> resources.</returns>
        public IAsyncEnumerable<IEnumerable<TResult>> CrossPartitionQueryAsync<TResult>(
            QueryDefinition query,
            CancellationToken cancellationToken = default);
    }
}