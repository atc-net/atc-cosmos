namespace Atc.Cosmos.Tests.Internal;

public sealed class CosmosWriterFactoryTests
{
    [Theory, AutoNSubstituteData]
    public void CreateWriter_Returns_NotNull(CosmosWriterFactory sut)
        => sut
            .CreateWriter<Record>()
            .Should()
            .NotBeNull();

    [Theory, AutoNSubstituteData]
    public void CreateBulkWriter_Returns_NotNull(CosmosWriterFactory sut)
        => sut
            .CreateBulkWriter<Record>()
            .Should()
            .NotBeNull();
}