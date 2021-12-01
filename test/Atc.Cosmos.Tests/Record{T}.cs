namespace Atc.Cosmos.Tests
{
    public sealed class Record<T> : ICosmosResource
    {
        public string Id { get; set; } = string.Empty;

        public string Pk { get; set; } = string.Empty;

        public string? ETag { get; set; }

        public T Data { get; set; }

        string ICosmosResource.DocumentId => Id;

        string ICosmosResource.PartitionKey => Pk;
    }
}