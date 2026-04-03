namespace Atc.Cosmos.Tests.Internal;

public sealed class ChangeFeedServiceTests
{
    private readonly IChangeFeedListener[] listeners;
    private readonly ChangeFeedService sut;

    public ChangeFeedServiceTests()
    {
        listeners = FixtureFactory.Create().Create<IChangeFeedListener[]>();

        sut = new ChangeFeedService(
            listeners);
    }

    [Fact]
    public void Should_Implement_IHostedService()
        => sut
            .Should()
            .BeAssignableTo<IHostedService>();

    [Theory, AutoNSubstituteData]
    public async Task StartAsync_Should_Call_Start_On_Listeners(
        CancellationToken cancellationToken)
    {
        await sut.StartAsync(cancellationToken);

        foreach (var listener in listeners)
        {
            _ = listener
                .Received(1)
                .StartAsync(cancellationToken);
        }
    }

    [Theory, AutoNSubstituteData]
    public async Task StopAsync_Should_Call_Stop_On_Listeners(
        CancellationToken cancellationToken)
    {
        await sut.StopAsync(cancellationToken);

        foreach (var listener in listeners)
        {
            _ = listener
                .Received(1)
                .StopAsync(cancellationToken);
        }
    }
}