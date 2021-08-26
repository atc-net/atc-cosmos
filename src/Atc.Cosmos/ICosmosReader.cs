﻿using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace Atc.Cosmos
{
    /// <summary>
    /// Represents a reader that can read Cosmos resources.
    /// </summary>
    /// <typeparam name="T">
    /// The type of <see cref="ICosmosResource"/>
    /// to be read by this reader.
    /// </typeparam>
    public interface ICosmosReader<T>
        where T : class, ICosmosResource
    {
        /// <summary>
        /// Attempts to read the specified <typeparamref name="T"/> resource,
        /// and returns <c>null</c> if none was found.
        /// </summary>
        /// <param name="documentId">Id of the resource.</param>
        /// <param name="partitionKey">Partition key of the resource.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used.</param>
        /// <returns>A <see cref="Task"/> containing the requested <typeparamref name="T"/> resource, or null.</returns>
        Task<T?> FindAsync(
            string documentId,
            string partitionKey,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Reads the specified <typeparamref name="T"/> resource from the configured
        /// Cosmos collection.
        /// </summary>
        /// <remarks>
        /// A <see cref="CosmosException"/>
        /// with StatusCode <see cref="HttpStatusCode.NotFound"/>
        /// will be thrown if resource could not be found.
        /// </remarks>
        /// <param name="documentId">Id of the resource.</param>
        /// <param name="partitionKey">Partition key of the resource.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used.</param>
        /// <returns>A <see cref="Task"/> the requested <typeparamref name="T"/> resource.</returns>
        public Task<T> ReadAsync(
            string documentId,
            string partitionKey,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Reads all the specified <typeparamref name="T"/> resource from the configured
        /// Cosmos collection.
        /// </summary>
        /// <param name="partitionKey">Partition key of the resource.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used.</param>
        /// <returns>An <see cref="IAsyncEnumerable&lt;T&gt;"/> over all the <typeparamref name="T"/> resources.</returns>
        public IAsyncEnumerable<T> ReadAllAsync(
            string partitionKey,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Query documents from the configured Cosmos container.
        /// </summary>
        /// <param name="query">Cosmos query to execute.</param>
        /// <param name="partitionKey">Partition key of the resource.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used.</param>
        /// <returns>An <see cref="IAsyncEnumerable&lt;T&gt;"/> over the requested <typeparamref name="T"/> resources.</returns>
        public IAsyncEnumerable<T> QueryAsync(
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
        public IAsyncEnumerable<TResult> QueryAsync<TResult>(
            QueryDefinition query,
            string partitionKey,
            CancellationToken cancellationToken = default);
    }
}