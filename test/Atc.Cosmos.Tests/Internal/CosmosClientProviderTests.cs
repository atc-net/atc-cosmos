namespace Atc.Cosmos.Tests.Internal;

public sealed class CosmosClientProviderTests : IDisposable
{
    private readonly CosmosOptions cosmosOptions;
    private readonly CosmosClientOptions cosmosClientOptions;
    private readonly IJsonCosmosSerializer serializer;
    private readonly CosmosClientProvider sut;

    public CosmosClientProviderTests()
    {
        var fixture = FixtureFactory.Create();

        cosmosOptions = new CosmosOptions
        {
            AccountEndpoint = fixture.Create<Uri>().AbsoluteUri,
            AccountKey = Convert.ToBase64String(Encoding.UTF8.GetBytes(fixture.Create<string>())),
            DatabaseName = fixture.Create<string>(),
            DatabaseThroughput = fixture.Create<int>(),
        };

        cosmosClientOptions = new CosmosClientOptions();
        serializer = Substitute.For<IJsonCosmosSerializer>();

        sut = new CosmosClientProvider(
            Options.Create(cosmosClientOptions),
            serializer);
    }

    public void Dispose()
        => sut.Dispose();

    [Fact]
    public void Clients_Should_Use_Endpoint_From_CosmosOptions()
        => sut
            .GetClient(cosmosOptions)
            .Endpoint
            .Should()
            .BeEquivalentTo(
                new Uri(cosmosOptions.AccountEndpoint));

    [Fact]
    public void Client_Should_Use_CosmosClientOptions()
        => sut
            .GetClient(cosmosOptions)
            .ClientOptions
            .Should()
            .BeEquivalentTo(
                cosmosClientOptions,
                o => o
                    .Excluding(o => o.AllowBulkExecution)
                    .Excluding(o => o.Serializer));

    [Fact]
    public void Client_Should_NotSet_CosmosClientOptions_ApplicationName_When_NotSpecified()
        => sut
            .GetClient(cosmosOptions)
            .ClientOptions
            .Should()
            .BeEquivalentTo(
                cosmosClientOptions,
                o => o
                    .Excluding(o => o.AllowBulkExecution)
                    .Excluding(o => o.Serializer));

    [Fact]
    public void Client_Should_Use_Default_Serializer_If_None_Specified()
    {
        // Arrange
        cosmosClientOptions.Serializer = null;

        // Act
        var actualSerializer = sut
            .GetClient(cosmosOptions)
            .ClientOptions
            .Serializer;

        // Assert
        actualSerializer.Should().BeAssignableTo<CosmosSerializerAdapter>();

        ((CosmosSerializerAdapter)actualSerializer)
           .Serializer
           .Should()
           .Be(serializer);
    }

    [Theory, AutoNSubstituteData]
    public void Client_Should_Use_CustomSerializer_If_Specified(
        [Substitute] CosmosSerializer serializer)
    {
        // Arrange
        cosmosClientOptions.Serializer = serializer;

        // Act & assert
        sut.GetClient(cosmosOptions).ClientOptions.Serializer.Should().Be(serializer);
    }

    [Fact]
    public void GetClient_Should_Return_Same_Instance()
        => sut
            .GetClient(cosmosOptions)
            .Should()
            .Be(sut.GetClient(cosmosOptions));

    [Fact]
    public void GetClient_And_GetBulkClinet_Should_Return_Different_Instances()
        => sut
            .GetClient(cosmosOptions)
            .Should()
            .NotBe(sut.GetBulkClient(cosmosOptions));

    [Fact]
    public void GetBulkClient_Should_Return_Same_Instance()
        => sut
            .GetBulkClient(cosmosOptions)
            .Should()
            .Be(sut.GetBulkClient(cosmosOptions));

    [Fact]
    public void BulkClient_Should_Use_Endpoint_From_CosmosOptions()
        => sut
            .GetBulkClient(cosmosOptions)
            .Endpoint
            .Should()
            .BeEquivalentTo(
                new Uri(cosmosOptions.AccountEndpoint));

    [Fact]
    public void BulkClient_Should_Use_CosmosClientOptions()
        => sut
            .GetBulkClient(cosmosOptions)
            .ClientOptions
            .Should()
            .BeEquivalentTo(
                cosmosClientOptions,
                o => o
                    .Excluding(o => o.AllowBulkExecution)
                    .Excluding(o => o.Serializer));

    [Fact]
    public void BulkClient_Should_Use_Default_Serializer_If_None_Specified()
    {
        // Arrange
        cosmosClientOptions.Serializer = null;

        // Act
        var actualSerializer = sut
            .GetBulkClient(cosmosOptions)
            .ClientOptions
            .Serializer;

        // Assert
        actualSerializer.Should().BeAssignableTo<CosmosSerializerAdapter>();

        ((CosmosSerializerAdapter)actualSerializer)
           .Serializer
           .Should()
           .Be(serializer);
    }

    [Theory, AutoNSubstituteData]
    public void BulkClient_Should_Use_CustomSerializer_If_Specified(
        [Substitute] CosmosSerializer serializer)
    {
        // Arrange
        cosmosClientOptions.Serializer = serializer;

        // Act & assert
        sut.GetBulkClient(cosmosOptions).ClientOptions.Serializer.Should().Be(serializer);
    }

    [Fact]
    public void Client_Should_Use_TokenCredential_When_Specified()
    {
        // Arrange
        cosmosOptions.Credential = new FakeTokenCredential();

        using var provider = new CosmosClientProvider(
            Options.Create(cosmosClientOptions),
            serializer);

        // Act & assert
        // As there is not possiblility to assert if a CosmosClient
        // has been instanciated with a TokenCredential or auth key
        // we simply ensure that we get a client object.
        provider.GetClient(cosmosOptions).Should().NotBeNull();
    }
}