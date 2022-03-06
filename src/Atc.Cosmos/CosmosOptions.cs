using System.Text.Json;
using Azure.Core;

namespace Atc.Cosmos
{
    /// <summary>
    /// Options for configuring the connection to Cosmos.
    /// </summary>
    public class CosmosOptions
    {
        /// <summary>
        /// Gets or sets the Cosmos account endpoint URI.
        /// </summary>
        /// <remarks>
        /// You can get this value from the Azure portal.
        /// Navigate to your Azure Cosmos account.
        /// Open the Overview pane and copy the URI value.
        /// </remarks>
        public string AccountEndpoint { get; set; } = default!;

        /// <summary>
        /// Gets or sets the Cosmos account key.
        /// </summary>
        /// <remarks>
        /// You can get this value from the Azure portal.
        /// Navigate to your Azure Cosmos account.
        /// Open the Connection Strings or Keys pane, and copy the
        /// "Password" or PRIMARY KEY value.
        /// The <see cref="AccountKey"/> is required if <see cref="Credential"/> is not specified.
        /// </remarks>
        public string? AccountKey { get; set; } = default!;

        /// <summary>
        /// Gets or sets the Cosmos database name.
        /// </summary>
        public string DatabaseName { get; set; } = default!;

        /// <summary>
        /// Gets or sets the throughput provisioned for a database in measurement
        /// of Request Units per second in the Azure Cosmos DB service.
        /// </summary>
        /// <remarks>
        /// Default value is 1000.
        /// </remarks>
        public int DatabaseThroughput { get; set; } = 1000;

        /// <summary>
        /// Gets or sets the <see cref="Microsoft.Azure.Cosmos.QueryRequestOptions.ResponseContinuationTokenLimitInKb"/>
        /// request option for <c>PagedQuery()</c> calls on <see cref="ICosmosReader{T}"/>.
        /// </summary>
        /// <remarks>
        /// ContinuationTokenLimitInKb is used to limit the length of continuation
        /// token in the query response. Valid values are >= 0.
        /// </remarks>
        public int? ContinuationTokenLimitInKb { get; set; }

        /// <summary>
        /// Gets or sets the options for controlling the json serializer.
        /// </summary>
        public JsonSerializerOptions SerializerOptions { get; set; }
            = new JsonSerializerOptions();

        /// <summary>
        /// Gets or sets the TokenCredential to use instead of <see cref="AccountKey"/>.
        /// </summary>
        /// <remarks>
        /// When <see cref="TokenCredential"/> is provided the property <see cref="AccountKey"/> is not needed.
        /// </remarks>
        public TokenCredential? Credential { get; set; }
    }
}