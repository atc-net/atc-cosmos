namespace Atc.Cosmos.Tests.DependencyInjection;

public sealed class CosmosContainerBuilderTests
{
    [Theory, AutoNSubstituteData]
    public void AddResource_Registers_ICosmosConntainerNameProvider(
        [Frozen] IServiceCollection services,
        [Frozen] ICosmosContainerNameProviderFactory registry,
        CosmosContainerBuilder sut)
    {
        sut.AddResource<Record>();

        services
            .Received(1)
            .Add(Arg.Is<ServiceDescriptor>(s
                => s.ServiceType
                == typeof(ICosmosContainerNameProvider)));

        registry
            .Received(1)
            .Register<Record>(sut.ContainerName, sut.Options);
    }

    // Test double registration will fail
    // Test same container in different databases will fail
}