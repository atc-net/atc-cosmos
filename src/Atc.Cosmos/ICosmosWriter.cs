using System;
using System.Threading;
using System.Threading.Tasks;

namespace Atc.Cosmos
{
    /// <summary>
    /// Represents a writer that can write Cosmos resources.
    /// </summary>
    /// <typeparam name="T">
    /// The type of <see cref="ICosmosResource"/>
    /// to be written by this writer.
    /// </typeparam>
    public interface ICosmosWriter<T>
        where T : class, ICosmosResource
    {
        /// <summary>
        /// Creates a new <typeparamref name="T"/> resource in Cosmos.
        /// </summary>
        /// <remarks>
        /// A <see cref="Microsoft.Azure.Cosmos.CosmosException"/>
        /// will be thrown if a resource already exists.
        /// </remarks>
        /// <param name="document">The resource to be created.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used.</param>
        /// <returns>A <see cref="Task"/> containing the created <typeparamref name="T"/> resource.</returns>
        Task<T> CreateAsync(
            T document,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Writes a <typeparamref name="T"/> resource to Cosmos, using upsert behavior.
        /// </summary>
        /// <param name="document">The resource to be written.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used.</param>
        /// <returns>A <see cref="Task"/> containing the written <typeparamref name="T"/> resource.</returns>
        Task<T> WriteAsync(
            T document,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Replaces a <typeparamref name="T"/> resource in Cosmos.
        /// </summary>
        /// <remarks>
        /// A <see cref="Microsoft.Azure.Cosmos.CosmosException"/>
        /// will be thrown if the resource does not already exist in Cosmos,
        /// or if the resource has been updated since it was read
        /// (using the <see cref="ICosmosResource.ETag"/>).
        /// </remarks>
        /// <param name="document">The resource to be created.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used.</param>
        /// <returns>A <see cref="Task"/> containing the updated <typeparamref name="T"/> resource.</returns>
        Task<T> ReplaceAsync(
            T document,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes the specified <typeparamref name="T"/> resource from Cosmos.
        /// </summary>
        /// <remarks>
        /// A <see cref="Microsoft.Azure.Cosmos.CosmosException"/>
        /// will be thrown if resource could not be found.
        /// </remarks>
        /// <param name="documentId">Id of the resource.</param>
        /// <param name="partitionKey">Partition key of the resource.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task DeleteAsync(
            string documentId,
            string partitionKey,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates a <typeparamref name="T"/> resource that is read from the configured
        /// Cosmos collection.
        /// </summary>
        /// <param name="documentId">Id of the resource.</param>
        /// <param name="partitionKey">Partition key of the resource.</param>
        /// <param name="updateDocument">Function for applying updates to the resource.</param>
        /// <param name="retries">Number of retries when a conflict occurs.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used.</param>
        /// <returns>A <see cref="Task"/> containing the updated <typeparamref name="T"/> resource.</returns>
        Task<T> UpdateAsync(
            string documentId,
            string partitionKey,
            Func<T, Task> updateDocument,
            int retries = 0,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates a <typeparamref name="T"/> resource that is read from the configured
        /// Cosmos collection.
        /// </summary>
        /// <param name="documentId">Id of the resource.</param>
        /// <param name="partitionKey">Partition key of the resource.</param>
        /// <param name="updateDocument">Function for applying updates to the resource.</param>
        /// <param name="retries">Number of retries when a conflict occurs.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used.</param>
        /// <returns>A <see cref="Task"/> containing the updated <typeparamref name="T"/> resource.</returns>
        Task<T> UpdateAsync(
            string documentId,
            string partitionKey,
            Action<T> updateDocument,
            int retries = 0,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates a <typeparamref name="T"/> resource that is read from the configured
        /// Cosmos collection, or creates it if it does not exist.
        /// </summary>
        /// <param name="getDefaultDocument">Function for creating the default resource. The returned resource need to have the DocumentId and PartitionKey set.</param>
        /// <param name="updateDocument">Function for applying updates to the resource.</param>
        /// <param name="retries">Number of retries when a conflict occurs.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used.</param>
        /// <returns>A <see cref="Task"/> containing the updated <typeparamref name="T"/> resource.</returns>
        Task<T> UpdateOrCreateAsync(
            Func<T> getDefaultDocument,
            Func<T, Task> updateDocument,
            int retries = 0,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates a <typeparamref name="T"/> resource that is read from the configured
        /// Cosmos collection, or creates it if it does not exist.
        /// </summary>
        /// <param name="getDefaultDocument">Function for creating the default resource. The returned resource need to have the DocumentId and PartitionKey set.</param>
        /// <param name="updateDocument">Function for applying updates to the resource.</param>
        /// <param name="retries">Number of retries when a conflict occurs.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used.</param>
        /// <returns>A <see cref="Task"/> containing the updated <typeparamref name="T"/> resource.</returns>
        Task<T> UpdateOrCreateAsync(
            Func<T> getDefaultDocument,
            Action<T> updateDocument,
            int retries = 0,
            CancellationToken cancellationToken = default);
    }
}