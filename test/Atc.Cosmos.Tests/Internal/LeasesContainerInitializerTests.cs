namespace Atc.Cosmos.Tests.Internal;

public sealed class LeasesContainerInitializerTests
{
    [Theory, AutoNSubstituteData]
    public async Task Should_Create_Cosmos_Container(
        LeasesContainerInitializer sut,
        Database database,
        CancellationToken cancellationToken)
    {
        // Act
        await sut.InitializeAsync(database, cancellationToken);

        // Assert
        _ = database
            .Received(1)
            .CreateContainerIfNotExistsAsync(
                Arg.Any<ContainerProperties>(),
                throughput: null,
                requestOptions: null,
                cancellationToken: cancellationToken);

        var options = database
            .ReceivedCallWithArgument<ContainerProperties>();

        options.IndexingPolicy.Automatic.Should().BeTrue();
        options.IndexingPolicy.IndexingMode.Should().Be(IndexingMode.Consistent);
        options.IndexingPolicy.ExcludedPaths.Should().ContainEquivalentOf(new ExcludedPath { Path = "/*" });
        options.Id.Should().Be("leases");
        options.PartitionKeyPath.Should().Be("/id");
    }
}