namespace Atc.Cosmos.Tests.DependencyInjection;

public sealed class ServiceCollectionExtensionsTests
{
    private readonly IJsonCosmosSerializer serializer;
    private readonly IOptions<CosmosOptions> options;
    private readonly IServiceCollection services;
    private readonly Action<ICosmosBuilder> builder;
    private readonly IServiceProvider provider;

    public ServiceCollectionExtensionsTests()
    {
        var fixture = FixtureFactory.Create();
        var cosmosOptions = fixture.Create<CosmosOptions>();
        cosmosOptions.UseCosmosEmulator();
        options = Options.Create(cosmosOptions);
        services = Substitute.For<IServiceCollection>();
        builder = Substitute.For<Action<ICosmosBuilder>>();
        serializer = Substitute.For<IJsonCosmosSerializer>();

        provider = Substitute.For<IServiceProvider>();

        provider
            .GetService(typeof(IOptions<CosmosOptions>))
            .Returns(options);

        provider
            .GetService(typeof(IJsonCosmosSerializer))
            .Returns(serializer);
    }

    [Fact]
    public void ConfigureCosmos_Calls_Builder_With_CosmosBuilder()
    {
        // Act
        services.ConfigureCosmos(builder);

        // Assert
        builder
            .Received(1)
            .Invoke(Arg.Any<CosmosBuilder>());
    }

    [Theory]
    [InlineData(typeof(ICosmosContainerRegistry))]
    [InlineData(typeof(ICosmosContainerNameProviderFactory))]
    [InlineData(typeof(ICosmosContainerProvider))]
    [InlineData(typeof(ICosmosReader<>))]
    [InlineData(typeof(ICosmosWriter<>))]
    [InlineData(typeof(ICosmosBulkWriter<>))]
    [InlineData(typeof(ICosmosInitializer))]
    [InlineData(typeof(IJsonCosmosSerializer))]
    [InlineData(typeof(ICosmosClientProvider))]
    [InlineData(typeof(ICosmosReaderFactory))]
    [InlineData(typeof(ICosmosWriterFactory))]
    public void ConfigureCosmos_Adds_Dependencies(Type serviceType)
    {
        // Act
        services.ConfigureCosmos(builder);

        // Assert
        services
            .Received(1)
            .Add(Arg.Is<ServiceDescriptor>(s
                => s.Lifetime == ServiceLifetime.Singleton
                && s.ServiceType == serviceType));
    }

    [Fact]
    public void ConfigureCosmos_Registers_CosmosOptions_If_Passed_In_By_Value()
    {
        // Act
        services.ConfigureCosmos(options.Value, builder);

        // Assert
        services
            .Received(1)
            .Add(Arg.Is<ServiceDescriptor>(s
                => s.ServiceType == typeof(IOptions<CosmosOptions>)));
    }

    [Fact]
    public void ConfigureCosmos_Registers_CosmosOptions_If_Passed_In_By_Function()
    {
        // Act
        services.ConfigureCosmos(s => options.Value, builder);

        // Assert
        services
            .Received(1)
            .Add(Arg.Is<ServiceDescriptor>(s
                => s.ServiceType == typeof(IOptions<CosmosOptions>)));
    }

    [Fact]
    public void ConfigureCosmos_Does_Not_Register_CosmosOptions_If_Not_Passed_In()
    {
        // Act
        services.ConfigureCosmos(builder);

        // Assert
        services
            .DidNotReceive()
            .Add(Arg.Is<ServiceDescriptor>(s
                => s.ServiceType == typeof(IOptions<CosmosOptions>)));
    }
}