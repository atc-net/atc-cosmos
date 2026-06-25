namespace Atc.Cosmos.Tests.AutoIncrement;

public sealed class CosmosBuilderExtensionsTests
{
    [Theory, AutoNSubstituteData]
    public void AddAutoIncrementProvider_Calls_AddContainer_On_Builder(
        ICosmosBuilder builder)
    {
        // Act
        builder.AddAutoIncrementProvider();

        // Assert
        builder
            .Received(1)
            .AddContainer<AutoIncrementCounterInitializer, AutoIncrementCounter>(
                AutoIncrementCounterInitializer.ContainerId);
    }

    [Theory, AutoNSubstituteData]
    public void AddAutoIncrementProvider_Registers_AutoIncrementProvider(
        ICosmosBuilder builder,
        IServiceCollection services)
    {
        // Arrange
        builder.Services.Returns(services);

        // Act
        builder.AddAutoIncrementProvider();

        // Assert
        services
            .Received(1)
            .Add(Arg.Is<ServiceDescriptor>(s
                => s.ServiceType == typeof(IAutoIncrementProvider)
                && s.ImplementationType == typeof(AutoIncrementProvider)));
    }
}