namespace Atc.Cosmos.Tests.Internal;

public sealed class StartupInitializationJobTests
{
    [Theory, AutoNSubstituteData]
    public async Task Should_Initialize_Cosmos_OnStart(
        [Frozen, Substitute] ICosmosInitializer initializer,
        StartupInitializationJob sut,
        CancellationToken cancellationToken)
    {
        // Act
        await sut.StartAsync(cancellationToken);

        // Assert
        await initializer
            .Received(1)
            .InitializeAsync(
                Arg.Any<CancellationToken>());
    }
}