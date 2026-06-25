namespace Atc.Cosmos.Tests.Internal;

public sealed class CosmosContainerProviderTests
{
    [Theory, AutoNSubstituteData]
    public void GetContainer_Return_Specified_Container(
        ICosmosClientProvider clientProvider,
        [Substitute] CosmosClient cosmosClient,
        OptionsWrapper<CosmosOptions> options,
        [Substitute] Container container,
        [Substitute] ICosmosContainerRegistry containerRegistry,
        string containerName)
    {
        // Arrange
        containerRegistry
            .DefaultOptions
            .Returns(options.Value);

        clientProvider
            .GetClient(options.Value)
            .Returns(cosmosClient);

        cosmosClient
            .GetContainer(databaseId: null, containerId: null)
            .ReturnsForAnyArgs(container);

        var sut = new CosmosContainerProvider(
            clientProvider,
            containerRegistry);

        // Act & assert
        sut.GetContainer(containerName).Should().Be(container);

        // Assert
        clientProvider
            .Received(1)
            .GetClient(options.Value);

        cosmosClient
            .Received(1)
            .GetContainer(
                options.Value.DatabaseName,
                containerName);
    }

    [Theory, AutoNSubstituteData]
    public void GetContainer_Return_NamedContainer(
        ICosmosClientProvider clientProvider,
        [Substitute] CosmosClient cosmosClient,
        OptionsWrapper<CosmosOptions> options,
        ICosmosContainerNameProvider provider,
        [Substitute] Container container,
        [Substitute] ICosmosContainerRegistry containerRegistry,
        string providerName)
    {
        // Arrange
        clientProvider
            .GetClient(options.Value)
            .Returns(cosmosClient);

        cosmosClient
            .GetContainer(databaseId: null, containerId: null)
            .ReturnsForAnyArgs(container);

        provider
            .IsForType(typeof(string))
            .Returns(true);

        provider
            .ContainerName
            .Returns(providerName);

        provider
            .Options
            .Returns(options.Value);

        containerRegistry
            .DefaultOptions
            .Returns(options.Value);

        containerRegistry
            .GetContainerForType<string>()
            .Returns(provider);

        var sut = new CosmosContainerProvider(
            clientProvider,
            containerRegistry);

        // Act & assert
        sut.GetContainer<string>().Should().Be(container);

        // Assert
        containerRegistry
            .Received(1)
            .GetContainerForType<string>();

        clientProvider
            .Received(1)
            .GetClient(options.Value);

        cosmosClient
            .Received(1)
            .GetContainer(
                options.Value.DatabaseName,
                providerName);
    }

    [Theory, AutoNSubstituteData]
    public void GetContainer_Of_Unsupported_Type_Throws_NotSupportedException(
        ICosmosClientProvider clientProvider,
        [Substitute] CosmosClient cosmosClient,
        OptionsWrapper<CosmosOptions> options,
        [Substitute] ICosmosContainerNameProvider nameProvider)
    {
        // Arrange
        clientProvider
            .GetClient(options.Value)
            .Returns(cosmosClient);

        nameProvider
            .IsForType(typeof(CosmosContainerProviderTests))
            .Returns(false);

        var containerRegistry = new CosmosContainerRegistry(options, new[] { nameProvider });

        var sut = new CosmosContainerProvider(
            clientProvider,
            containerRegistry);

        // Act & assert
        new Action(() => sut.GetContainer<CosmosContainerProviderTests>())
            .Should()
            .ThrowExactly<NotSupportedException>();

        // Assert
        cosmosClient
            .DidNotReceive()
            .GetContainer(Arg.Any<string>(), Arg.Any<string>());
    }

    [Theory, AutoNSubstituteData]
    public void GetContainer_For_Bulk_Return_Specified_Container(
        ICosmosClientProvider clientProvider,
        [Substitute] CosmosClient cosmosClient,
        OptionsWrapper<CosmosOptions> options,
        [Substitute] Container container,
        [Substitute] ICosmosContainerRegistry containerRegistry,
        string containerName)
    {
        // Arrange
        clientProvider
            .GetBulkClient(options.Value)
            .Returns(cosmosClient);

        cosmosClient
            .GetContainer(databaseId: null, containerId: null)
            .ReturnsForAnyArgs(container);

        containerRegistry
            .DefaultOptions
            .Returns(options.Value);

        var sut = new CosmosContainerProvider(
            clientProvider,
            containerRegistry);

        // Act & assert
        sut.GetContainer(containerName, allowBulk: true).Should().Be(container);

        // Assert
        clientProvider
            .Received(1)
            .GetBulkClient(options.Value);

        cosmosClient
            .Received(1)
            .GetContainer(
                options.Value.DatabaseName,
                containerName);
    }

    [Theory, AutoNSubstituteData]
    public void GetContainer_For_Bulk_Returns_NamedContainer(
        ICosmosClientProvider clientProvider,
        [Substitute] CosmosClient cosmosClient,
        OptionsWrapper<CosmosOptions> options,
        ICosmosContainerNameProvider provider,
        [Substitute] Container container,
        string providerName)
    {
        // Arrange
        clientProvider
            .GetBulkClient(options.Value)
            .Returns(cosmosClient);

        cosmosClient
            .GetContainer(databaseId: null, containerId: null)
            .ReturnsForAnyArgs(container);

        provider
            .IsForType(typeof(string))
            .Returns(true);

        provider
            .ContainerName
            .Returns(providerName);

        provider
            .Options
            .Returns(options.Value);

        var containerRegistry = new CosmosContainerRegistry(options, new[] { provider });

        var sut = new CosmosContainerProvider(
            clientProvider,
            containerRegistry);

        // Act & assert
        sut.GetContainer<string>(allowBulk: true).Should().Be(container);

        // Assert
        clientProvider
            .Received(1)
            .GetBulkClient(options.Value);

        cosmosClient
            .Received(1)
            .GetContainer(
                options.Value.DatabaseName,
                providerName);
    }

    [Theory, AutoNSubstituteData]
    public void GetContainer_For_Bulk_Of_Unsupported_Type_Throws_NotSupportedException(
        ICosmosClientProvider clientProvider,
        [Substitute] CosmosClient cosmosClient,
        OptionsWrapper<CosmosOptions> options,
        [Substitute] ICosmosContainerNameProvider nameProvider)
    {
        // Arrange
        clientProvider
            .GetBulkClient(options.Value)
            .Returns(cosmosClient);

        nameProvider
            .IsForType(typeof(CosmosContainerProviderTests))
            .Returns(false);

        var containerRegistry = new CosmosContainerRegistry(options, new[] { nameProvider });

        var sut = new CosmosContainerProvider(
            clientProvider,
            containerRegistry);

        // Act & assert
        new Action(() => sut.GetContainer<CosmosContainerProviderTests>(allowBulk: true))
            .Should()
            .ThrowExactly<NotSupportedException>();

        // Assert
        cosmosClient
            .DidNotReceive()
            .GetContainer(Arg.Any<string>(), Arg.Any<string>());
    }
}