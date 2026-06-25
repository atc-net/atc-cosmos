namespace Atc.Cosmos.Tests.DependencyInjection;

public sealed class CosmosContainerBuilderTests
{
    [Theory, AutoNSubstituteData]
    public void AddResource_Registers_ICosmosContainerNameProvider(
        [Frozen] IServiceCollection services,
        [Frozen] ICosmosContainerNameProviderFactory registry,
        CosmosContainerBuilder sut)
    {
        // Act
        sut.AddResource<Record>();

        // Assert
        services
            .Received(1)
            .Add(Arg.Is<ServiceDescriptor>(s
                => s.ServiceType
                == typeof(ICosmosContainerNameProvider)));

        registry
            .Received(1)
            .Register<Record>(sut.ContainerName, sut.Options);
    }
}