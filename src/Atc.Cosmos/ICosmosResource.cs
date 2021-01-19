using Atc.Cosmos.Internal;

namespace Atc.Cosmos
{
    /// <summary>
    /// Represents a resource that can exist as a document in a Cosmos collection.
    /// </summary>
    public interface ICosmosResource
    {
        /// <summary>
        /// Gets or sets the ETag used for version checking when updating the
        /// resources.
        /// </summary>
        /// <remarks>
        /// Do NOT set this property unless you know what you are doing.
        /// The <see cref="CosmosWriter{T}"/> relies on the ETag being unchanged
        /// since the last read.
        /// </remarks>
        string? ETag { get; set; }

        /// <summary>
        /// Gets the id of the Cosmos document.
        /// </summary>
        string DocumentId { get; }

        /// <summary>
        /// Gets the partition key of the Cosmos document.
        /// </summary>
        string PartitionKey { get; }
    }
}