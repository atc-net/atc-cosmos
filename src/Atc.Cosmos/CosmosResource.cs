using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Atc.Cosmos
{
    /// <summary>
    /// Abstract base-class for Cosmos resources.
    /// </summary>
    /// <remarks>
    /// By using this implementation of the <see cref="ICosmosResource"/>
    /// interface, the public interface of the class is not polluted with
    /// Cosmos specific properties, as these are implemented explicitly.
    /// </remarks>
    [SuppressMessage(
        "Design",
        "CA1033:Interface methods should be callable by child types",
        Justification = "By design")]
    public abstract class CosmosResource : ICosmosResource
    {
        [JsonIgnore]
        string ICosmosResource.DocumentId => GetDocumentId();

        [JsonIgnore]
        string ICosmosResource.PartitionKey => GetPartitionKey();

        [JsonIgnore]
        string? ICosmosResource.ETag { get; set; }

        /// <summary>
        /// Method for getting the id used for the document.
        /// </summary>
        /// <remarks>
        /// Implement this by returning the property used as the document id.
        /// </remarks>
        /// <returns>The document id.</returns>
        protected abstract string GetDocumentId();

        /// <summary>
        /// Method for getting the partition key used for the document.
        /// </summary>
        /// <remarks>
        /// Implement this by returning the property used as the partition key.
        /// </remarks>
        /// <returns>The partition key.</returns>
        protected abstract string GetPartitionKey();
    }
}