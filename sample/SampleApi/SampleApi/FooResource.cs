namespace SampleApi;

public class FooResource : CosmosResource
{
    public const string PartitionKey = "foo";

    public string Id { get; set; } = null!;

    public string Pk { get; } = PartitionKey;

    public Dictionary<string, object> Data { get; set; } = [];

    protected override string GetDocumentId() => Id;

    protected override string GetPartitionKey() => Pk;
}