using Atc.Cosmos;

namespace SampleApi;

public class FooResource : CosmosResource
{
    public const string PartitionKey = "foo";

    public string Id { get; set; } = null!;

    public string Pk => PartitionKey;

    public Dictionary<string, object> Bar { get; set; } = new Dictionary<string, object>();

    protected override string GetDocumentId() => Id;

    protected override string GetPartitionKey() => Pk;
}