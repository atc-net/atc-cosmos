namespace Atc.Cosmos.Tests
{
    public sealed class Record : ICosmosResource
    {
        public string Id { get; set; } = string.Empty;

        public string Pk { get; set; } = string.Empty;

        public string? ETag { get; set; }

        public string Data { get; set; }

        string ICosmosResource.DocumentId => Id;

        string ICosmosResource.PartitionKey => Pk;
    }
}