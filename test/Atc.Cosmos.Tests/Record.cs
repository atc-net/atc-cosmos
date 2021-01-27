namespace Atc.Cosmos.Tests
{
    public class Record : CosmosResource
    {
        public string Id { get; set; } = string.Empty;

        public string Pk { get; set; } = string.Empty;

        protected override string GetDocumentId()
            => Id;

        protected override string GetPartitionKey()
            => Pk;
    }
}