using Atc.Cosmos.Internal;

namespace Atc.Cosmos
{
    /// <summary>
    /// Represents a resource that can exist as a document in a Cosmos collection.
    /// </summary>
    public interface ICosmosResource
    {
        /// <summary>
        /// ETag used for version checking the resource before
        /// updating when using <see cref="ICosmosWriter{T}.ReplaceAsync(T, System.Threading.CancellationToken)"/>.
        /// </summary>
        /// <remarks>
        /// Do NOT set this property unless you know what you are doing.
        /// The <see cref="CosmosWriter{T}"/> relies on the ETag being unchanged since the last read.
        /// </remarks>
        string ETag { get; set; }

        /// <summary>
        /// The id of the Cosmos document.
        /// </summary>
        string DocumentId { get; }

        /// <summary>
        /// The partition key of the Cosmos document.
        /// </summary>
        string PartitionKey { get; }
    }
}