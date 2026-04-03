namespace Atc.Cosmos.Tests.Internal;

public sealed class CosmosContainerRegistryTests
{
    private readonly CosmosOptions cosmosOptions;

    public CosmosContainerRegistryTests()
    {
        var fixture = FixtureFactory.Create();
        cosmosOptions = new CosmosOptions
        {
            AccountEndpoint = fixture.Create<Uri>().AbsoluteUri,
            AccountKey = Convert.ToBase64String(Encoding.UTF8.GetBytes(fixture.Create<string>())),
            DatabaseName = fixture.Create<string>(),
            DatabaseThroughput = fixture.Create<int>(),
        };
    }

    [Fact]
    public void ShouldThrow_When_TokenCredential_And_AccountKey_IsMissing()
    {
        cosmosOptions.Credential = null;
        cosmosOptions.AccountKey = string.Empty;

        FluentActions.Invoking(
            () => new CosmosContainerRegistry(
                Options.Create(cosmosOptions),
                Enumerable.Empty<ICosmosContainerNameProvider>()))
            .Should()
            .Throw<InvalidOperationException>();
    }

    [Fact]
    public void ShouldThrow_When_No_AccountEndpoint_IsConfigured()
    {
        cosmosOptions.AccountEndpoint = string.Empty;

        FluentActions.Invoking(
            () => new CosmosContainerRegistry(
                Options.Create(cosmosOptions),
                Enumerable.Empty<ICosmosContainerNameProvider>()))
            .Should()
            .Throw<InvalidOperationException>();
    }

    [Fact]
    public void ShouldThrow_When_No_DatabaseName_IsConfigured()
    {
        cosmosOptions.DatabaseName = string.Empty;

        FluentActions.Invoking(
            () => new CosmosContainerRegistry(
                Options.Create(cosmosOptions),
                Enumerable.Empty<ICosmosContainerNameProvider>()))
            .Should()
            .Throw<InvalidOperationException>();
    }

    [Theory, AutoNSubstituteData]
    public void GetContainerForType_Of_Unsupported_Type_Throws_NotSupportedException(
        OptionsWrapper<CosmosOptions> options,
        [Substitute] ICosmosContainerNameProvider nameProvider)
    {
        nameProvider
            .IsForType(typeof(CosmosContainerProviderTests))
            .Returns(false);

        var sut = new CosmosContainerRegistry(
            options,
            new[] { nameProvider });

        new Action(() => sut.GetContainerForType<CosmosContainerProviderTests>())
            .Should()
            .ThrowExactly<NotSupportedException>();

        new Action(() => sut.GetContainerForType(typeof(CosmosContainerProviderTests)))
            .Should()
            .ThrowExactly<NotSupportedException>();
    }
}