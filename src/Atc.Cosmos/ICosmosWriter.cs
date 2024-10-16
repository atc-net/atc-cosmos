using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

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
        /// A <see cref="CosmosException"/>
        /// with StatusCode <see cref="HttpStatusCode.Conflict"/>
        /// will be thrown if a resource already exists.
        /// </remarks>
        /// <param name="document">The resource to be created.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used.</param>
        /// <returns>A <see cref="Task"/> containing the created <typeparamref name="T"/> resource.</returns>
        Task<T> CreateAsync(
            T document,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new <typeparamref name="T"/> resource in Cosmos.
        /// </summary>
        /// <remarks>
        /// This is optimal for workloads where the returned resource is not used.
        /// A <see cref="CosmosException"/>
        /// with StatusCode <see cref="HttpStatusCode.Conflict"/>
        /// will be thrown if a resource already exists.
        /// </remarks>
        /// <param name="document">The resource to be created.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task CreateWithNoResponseAsync(
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
        /// Writes a <typeparamref name="T"/> resource to Cosmos, using upsert behavior.
        /// </summary>
        /// <remarks>
        /// This is optimal for workloads where the returned resource is not used.
        /// </remarks>
        /// <param name="document">The resource to be written.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task WriteWithNoResponseAsync(
            T document,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Replaces a <typeparamref name="T"/> resource in Cosmos.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A <see cref="CosmosException"/>
        /// with StatusCode <see cref="HttpStatusCode.NotFound"/>
        /// will be thrown if the resource does not already exist in Cosmos.
        /// </para>
        /// <para>
        /// A <see cref="CosmosException"/>
        /// with StatusCode <see cref="HttpStatusCode.PreconditionFailed"/>
        /// will be thrown if the resource has been updated since it was read
        /// (using the <see cref="ICosmosResource.ETag"/> to match the version).
        /// </para>
        /// </remarks>
        /// <param name="document">The resource to be created.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used.</param>
        /// <returns>A <see cref="Task"/> containing the updated <typeparamref name="T"/> resource.</returns>
        Task<T> ReplaceAsync(
            T document,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Replaces a <typeparamref name="T"/> resource in Cosmos.
        /// </summary>
        /// <remarks>
        /// This is optimal for workloads where the returned resource is not used.
        /// <para>
        /// A <see cref="CosmosException"/>
        /// with StatusCode <see cref="HttpStatusCode.NotFound"/>
        /// will be thrown if the resource does not already exist in Cosmos.
        /// </para>
        /// <para>
        /// A <see cref="CosmosException"/>
        /// with StatusCode <see cref="HttpStatusCode.PreconditionFailed"/>
        /// will be thrown if the resource has been updated since it was read
        /// (using the <see cref="ICosmosResource.ETag"/> to match the version).
        /// </para>
        /// </remarks>
        /// <param name="document">The resource to be created.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task ReplaceWithNoResponseAsync(
            T document,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes the specified <typeparamref name="T"/> resource from Cosmos.
        /// </summary>
        /// <remarks>
        /// A <see cref="CosmosException"/>
        /// with StatusCode <see cref="HttpStatusCode.NotFound"/>
        /// will be thrown if the resource does not already exist in Cosmos.
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
        /// Tries to delete the specified <typeparamref name="T"/> resource from Cosmos.
        /// </summary>
        /// <remarks>
        /// When trying to delete a non existing resource, False is returned.
        /// </remarks>
        /// <param name="documentId">Id of the resource.</param>
        /// <param name="partitionKey">Partition key of the resource.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used.</param>
        /// <returns>True if resource was deleted otherwise False.</returns>
        public Task<bool> TryDeleteAsync(
            string documentId,
            string partitionKey,
            CancellationToken cancellationToken = default);
#if PREVIEW

        /// <summary>
        /// Preview Feature DeleteAllItemsByPartitionKey.<br/>
        /// Deletes all resources in the Container with the specified <see cref="PartitionKey"/>.
        /// Starts an asynchronous Cosmos DB background operation which deletes all resources in the Container with the specified value.
        /// The asynchronous Cosmos DB background operation runs using a percentage of user RUs.
        /// </summary>
        /// <remarks>
        /// A <see cref="CosmosException"/>
        /// with StatusCode <see cref="HttpStatusCode.BadRequest"/>
        /// will be thrown if the DeleteAllItemsByPartitionKey feature is not enabled.
        /// </remarks>
        /// <param name="partitionKey">Partition key of the resource.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task DeletePartitionAsync(
            string partitionKey,
            CancellationToken cancellationToken = default);
#endif

        /// <summary>
        /// Updates a <typeparamref name="T"/> resource that is read from the configured
        /// Cosmos collection.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A <see cref="CosmosException"/>
        /// with StatusCode <see cref="HttpStatusCode.NotFound"/>
        /// will be thrown if the resource does not already exist in Cosmos.
        /// </para>
        /// <para>
        /// A <see cref="CosmosException"/>
        /// with StatusCode <see cref="HttpStatusCode.PreconditionFailed"/>
        /// will be thrown if the resource is being updated simultanious by another thread
        /// and the <paramref name="retries"/> has run out.
        /// </para>
        /// </remarks>
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
        /// <remarks>
        /// <para>
        /// A <see cref="CosmosException"/>
        /// with StatusCode <see cref="HttpStatusCode.NotFound"/>
        /// will be thrown if the resource does not already exist in Cosmos.
        /// </para>
        /// <para>
        /// A <see cref="CosmosException"/>
        /// with StatusCode <see cref="HttpStatusCode.PreconditionFailed"/>
        /// will be thrown if the resource is being updated simultanious by another thread
        /// and the <paramref name="retries"/> has run out.
        /// </para>
        /// </remarks>
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
        /// <remarks>
        /// A <see cref="CosmosException"/>
        /// with StatusCode <see cref="HttpStatusCode.PreconditionFailed"/>
        /// will be thrown if the resource is being updated simultanious by another thread
        /// and the <paramref name="retries"/> has run out.
        /// </remarks>
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
        /// <remarks>
        /// A <see cref="CosmosException"/>
        /// with StatusCode <see cref="HttpStatusCode.PreconditionFailed"/>
        /// will be thrown if the resource is being updated simultanious by another thread
        /// and the <paramref name="retries"/> has run out.
        /// </remarks>
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

        /// <summary>
        /// Partially update a <typeparamref name="T"/> resource in Cosmos.
        /// </summary>
        /// <param name="documentId">Id of the resource.</param>
        /// <param name="partitionKey">Partition key of the resource.</param>
        /// <param name="patchOperations">Represents a list of operations to be sequentially applied to the referred Cosmos resource.</param>
        /// <param name="filterPredicate">
        /// Condition to be checked before the patch operations in the Azure Cosmos DB service are applied.
        /// A <see cref="CosmosException"/> with StatusCode <see cref="HttpStatusCode.PreconditionFailed"/>
        /// will be thrown if the condition is not met.
        /// <example><code>FROM c WHERE c.taskNum = 3</code></example>
        /// </param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used.</param>
        /// <returns>A <see cref="Task"/> containing the modified <typeparamref name="T"/> resource.</returns>
        Task<T> PatchAsync(
            string documentId,
            string partitionKey,
            IReadOnlyList<PatchOperation> patchOperations,
            string? filterPredicate = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Partially update a <typeparamref name="T"/> resource in Cosmos.
        /// </summary>
        /// <param name="documentId">Id of the resource.</param>
        /// <param name="partitionKey">Partition key of the resource.</param>
        /// <param name="patchOperations">Represents a list of operations to be sequentially applied to the referred Cosmos resource.</param>
        /// <param name="filterPredicate">
        /// Condition to be checked before the patch operations in the Azure Cosmos DB service are applied.
        /// A <see cref="CosmosException"/> with StatusCode <see cref="HttpStatusCode.PreconditionFailed"/>
        /// will be thrown if the condition is not met.
        /// <example><code>FROM c WHERE c.taskNum = 3</code></example>
        /// </param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task PatchWithNoResponseAsync(
            string documentId,
            string partitionKey,
            IReadOnlyList<PatchOperation> patchOperations,
            string? filterPredicate = null,
            CancellationToken cancellationToken = default);
    }
}