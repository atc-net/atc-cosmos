using System.Text.Json.Serialization;

namespace Atc.Cosmos.AutoIncrement
{
    public class AutoIncrementCounter : CosmosResource
    {
        [JsonPropertyName("id")]
        public string CounterName { get; set; }

        public int Count { get; set; }

        protected override string GetDocumentId()
            => CounterName;

        protected override string GetPartitionKey()
            => CounterName;
    }
}