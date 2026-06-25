namespace Atc.Cosmos.Tests.AutoIncrement;

public sealed class AutoIncrementCounterTests
{
    [Theory, AutoData]
    public void Implements_ICosmosResource(AutoIncrementCounter sut)
        => sut.Should().BeAssignableTo<CosmosResource>();

    [Theory, AutoData]
    public void DocumentId_Should_Be_CounterName(AutoIncrementCounter sut)
        => (sut as ICosmosResource)
            .DocumentId
            .Should()
            .Be(sut.CounterName);

    [Theory, AutoData]
    public void DocumentId_Should_Be_PartitionKey(AutoIncrementCounter sut)
        => (sut as ICosmosResource)
            .PartitionKey
            .Should()
            .Be(sut.CounterName);
}