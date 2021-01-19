using Atc.Cosmos;

namespace Atc.Cosmos.Tests
{
    public class Record : CosmosResource
    {
        public string Id { get; set; }
        public string Pk { get; set; }

        protected override string GetDocumentId()
            => Id;

        protected override string GetPartitionKey()
            => Pk;
    }
}