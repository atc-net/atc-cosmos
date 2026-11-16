namespace Atc.Cosmos.Functions.Testing.Tests;

public sealed class CosmosTriggerBindingVerifierTests
{
    private static readonly System.Reflection.Assembly FunctionsAssembly = typeof(ListTriggerFunction).Assembly;

    private static readonly JsonSerializerOptions CosmosSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() },
    };

    [Fact]
    public async Task VerifyAsync_Should_Fail_When_Worker_Reads_Enums_Written_As_Text_With_Its_Default_Serializer()
    {
        // Arrange
        var sut = CreateVerifier(_ => { });

        // Act
        var actual = await sut.VerifyAsync(FunctionsAssembly, ListTriggerFunction.Name, TestContext.Current.CancellationToken);

        // Assert
        actual.IsSuccess
            .Should()
            .BeFalse();

        actual.Exception
            .Should()
            .BeAssignableTo<JsonException>();

        actual.Message
            .Should()
            .Contain(ListTriggerFunction.Name)
            .And.Contain(nameof(SampleDocument))
            .And.Contain("$[0].type");
    }

    [Fact]
    public async Task VerifyAsync_Should_Succeed_When_WorkerOptions_Serializer_Uses_The_Cosmos_Serializer_Options()
    {
        // Arrange
        var sut = CreateVerifier(services => services.Configure<WorkerOptions>(
            options => options.Serializer = new JsonObjectSerializer(CosmosSerializerOptions)));

        // Act
        var actual = await sut.VerifyAsync(FunctionsAssembly, ListTriggerFunction.Name, TestContext.Current.CancellationToken);

        // Assert
        actual.IsSuccess
            .Should()
            .BeTrue(actual.Message);
    }

    [Fact]
    public async Task VerifyAsync_Should_Succeed_When_Parameter_Has_An_InputConverter_With_The_Cosmos_Serializer_Options()
    {
        // Arrange
        var sut = CreateVerifier(services => services.AddSingleton(CosmosSerializerOptions));

        // Act
        var actual = await sut.VerifyAsync(FunctionsAssembly, InputConverterTriggerFunction.Name, TestContext.Current.CancellationToken);

        // Assert
        actual.IsSuccess
            .Should()
            .BeTrue(actual.Message);
    }

    [Fact]
    public async Task VerifyAsync_Should_Fail_When_Worker_Reads_Different_Values_Without_An_Exception()
    {
        // Arrange
        var snakeCaseOptions = new JsonSerializerOptions(CosmosSerializerOptions)
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        };

        var sut = CreateVerifier(
            services => services.Configure<WorkerOptions>(
                options => options.Serializer = new JsonObjectSerializer(CosmosSerializerOptions)),
            snakeCaseOptions);

        // Act
        var actual = await sut.VerifyAsync(FunctionsAssembly, ListTriggerFunction.Name, TestContext.Current.CancellationToken);

        // Assert
        actual.IsSuccess
            .Should()
            .BeFalse();

        actual.Exception
            .Should()
            .BeNull();

        actual.Message
            .Should()
            .Contain(ListTriggerFunction.Name)
            .And.Contain("display_name");
    }

    [Fact]
    public async Task VerifyAsync_Should_Return_The_Json_Written_Like_Atc_Cosmos()
    {
        // Arrange
        var sut = CreateVerifier(_ => { });

        // Act
        var actual = await sut.VerifyAsync(FunctionsAssembly, ListTriggerFunction.Name, TestContext.Current.CancellationToken);

        // Assert
        actual.WrittenJson
            .Should()
            .StartWith("[")
            .And.Contain("\"displayName\"")
            .And.Contain("\"type\":\"");
    }

    [Fact]
    public Task VerifyAsync_Should_Throw_When_Assembly_Has_No_Cosmos_Trigger_With_The_Function_Name()
        => FluentActions
            .Awaiting(() => CreateVerifier(_ => { })
                .VerifyAsync(FunctionsAssembly, NonCosmosFunction.Name, TestContext.Current.CancellationToken))
            .Should()
            .ThrowAsync<ArgumentException>()
            .WithMessage($"*{NonCosmosFunction.Name}*");

    private static CosmosTriggerBindingVerifier CreateVerifier(
        Action<IServiceCollection> configureServices,
        JsonSerializerOptions? cosmosSerializerOptions = null)
    {
        var fixture = new Fixture();

        return new CosmosTriggerBindingVerifier(
            services =>
            {
                services.Configure<CosmosOptions>(
                    options => options.SerializerOptions = cosmosSerializerOptions ?? CosmosSerializerOptions);
                configureServices(services);
            },
            type => new SpecimenContext(fixture).Resolve(type));
    }
}