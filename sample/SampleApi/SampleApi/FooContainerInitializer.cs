namespace SampleApi;

public class FooContainerInitializer : ICosmosContainerInitializer
{
    public Task InitializeAsync(
        Database database,
        CancellationToken cancellationToken) =>
        database.CreateContainerIfNotExistsAsync(
            new ContainerProperties
            {
                PartitionKeyPath = "/pk",
                Id = "foo",
            },
            cancellationToken: cancellationToken);
}