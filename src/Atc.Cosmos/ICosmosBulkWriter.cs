using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace Atc.Cosmos
{
    /// <summary>
    /// Represents a writer that can perform bulk operations on Cosmos resources.
    /// </summary>
    /// <typeparam name="T">
    /// The type of <see cref="ICosmosResource"/>
    /// to be written by this writer.
    /// </typeparam>
    public interface ICosmosBulkWriter<in T>
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
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task CreateAsync(
            T document,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Writes a <typeparamref name="T"/> resource to Cosmos, using upsert behavior.
        /// </summary>
        /// <param name="document">The resource to be written.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task WriteAsync(
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
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task ReplaceAsync(
            T document,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes the specified <typeparamref name="T"/> resource from Cosmos.
        /// </summary>
        /// <remarks>
        /// A <see cref="CosmosException"/>
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
    }
}