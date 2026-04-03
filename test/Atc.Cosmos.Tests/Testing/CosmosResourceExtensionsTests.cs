namespace Atc.Cosmos.Tests.Testing;

public sealed class CosmosResourceExtensionsTests
{
    [Theory, AutoNSubstituteData]
    public void Should_Create_Clone(Record record)
    {
        record
            .Clone()
            .Should()
            .NotBeSameAs(record)
            .And
            .BeEquivalentTo(record);
    }

    [Theory, AutoNSubstituteData]
    public void Should_Create_Clone_Of_Nullable(Record? record)
    {
        record
            .Clone()
            .Should()
            .NotBeSameAs(record)
            .And
            .BeEquivalentTo(record);
    }
}