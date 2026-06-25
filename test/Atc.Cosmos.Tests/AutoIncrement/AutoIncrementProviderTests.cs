namespace Atc.Cosmos.Tests.AutoIncrement;

public sealed class AutoIncrementProviderTests
{
    private readonly ICosmosWriter<AutoIncrementCounter> writer;
    private readonly AutoIncrementProvider sut;
    private readonly AutoIncrementCounter counter;

    public AutoIncrementProviderTests()
    {
        counter = new Fixture().Create<AutoIncrementCounter>();
        writer = Substitute.For<ICosmosWriter<AutoIncrementCounter>>();

        writer
            .UpdateOrCreateAsync(
                Arg.Any<Func<AutoIncrementCounter>>(),
                Arg.Any<Action<AutoIncrementCounter>>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(counter);

        sut = new AutoIncrementProvider(writer);
    }

    [Theory, AutoNSubstituteData]
    public async Task GetNextAsync_Calls_UpdateOrCreateAsync(
        string counterName,
        CancellationToken cancellationToken)
    {
        // Act
        await sut.GetNextAsync(counterName, cancellationToken);

        // Assert
        _ = writer
            .Received(1)
            .UpdateOrCreateAsync(
                Arg.Any<Func<AutoIncrementCounter>>(),
                Arg.Any<Action<AutoIncrementCounter>>(),
                retries: 5,
                cancellationToken);
    }

    [Theory, AutoNSubstituteData]
    public async Task GetNextAsync_Calls_UpdateOrCreateAsync_With_Correct_Factory(
        string counterName,
        CancellationToken cancellationToken)
    {
        // Act
        await sut.GetNextAsync(counterName, cancellationToken);

        // Assert
        writer
            .ReceivedCallWithArgument<Func<AutoIncrementCounter>>()
            .Invoke()
            .Should()
            .BeEquivalentTo(new AutoIncrementCounter
            {
                CounterName = counterName,
            });
    }

    [Theory, AutoNSubstituteData]
    public async Task GetNextAsync_Calls_UpdateOrCreateAsync_With_Correct_Updater(
        string counterName,
        AutoIncrementCounter counterToUpdate,
        CancellationToken cancellationToken)
    {
        // Act
        await sut.GetNextAsync(counterName, cancellationToken);

        // Assert
        var expectedCount = counterToUpdate.Count + 1;

        writer
            .ReceivedCallWithArgument<Action<AutoIncrementCounter>>()
            .Invoke(counterToUpdate);

        counterToUpdate.Count.Should().Be(expectedCount);
    }

    [Theory, AutoNSubstituteData]
    public async Task GetNextAsync_Returns_Updated_Count(
        string counterName,
        int updatedCount,
        CancellationToken cancellationToken)
    {
        // Arrange
        counter.Count = updatedCount;

        // Act
        var result = await sut.GetNextAsync(counterName, cancellationToken);

        // Assert
        result.Should().Be(updatedCount);
    }
}