namespace Atc.Cosmos.Tests.Internal;

public sealed class CosmosInitializerTests
{
    private readonly ICosmosClientProvider clientProvider;
    private readonly CosmosClient client;
    private readonly CosmosClient client2;
    private readonly Database database;
    private readonly Database database2;
    private readonly CosmosOptions options;
    private readonly CosmosOptions secondOptions;
    private readonly ICosmosContainerRegistry containerRegistry;

    public CosmosInitializerTests()
    {
        clientProvider = Substitute.For<ICosmosClientProvider>();
        client = Substitute.For<CosmosClient>();
        client2 = Substitute.For<CosmosClient>();
        database = Substitute.For<Database>();
        database2 = Substitute.For<Database>();
        var databaseResponse1 = Substitute.For<DatabaseResponse>();
        var databaseResponse3 = Substitute.For<DatabaseResponse>();
        var containerResponse1 = Substitute.For<ContainerResponse>();
        options = Substitute.For<CosmosOptions>();
        secondOptions = Substitute.For<CosmosOptions>();
        secondOptions.DatabaseName = "name2";
        secondOptions.DatabaseThroughput = 10;
        containerRegistry = Substitute.For<ICosmosContainerRegistry>();

        clientProvider
            .GetClient(options)
            .Returns(client);

        clientProvider
            .GetClient(secondOptions)
            .Returns(client2);

        client
            .CreateDatabaseIfNotExistsAsync(options.DatabaseName, throughput: options.DatabaseThroughput, requestOptions: null, CancellationToken.None)
            .ReturnsForAnyArgs(databaseResponse1);

        databaseResponse1
            .Database
            .Returns(database);

        database
            .CreateContainerIfNotExistsAsync(containerProperties: null, throughput: null, requestOptions: null, CancellationToken.None)
            .ReturnsForAnyArgs(containerResponse1);

        client2
            .CreateDatabaseIfNotExistsAsync(secondOptions.DatabaseName, throughput: secondOptions.DatabaseThroughput, requestOptions: null, CancellationToken.None)
            .ReturnsForAnyArgs(databaseResponse3);

        databaseResponse3
            .Database
            .Returns(database2);

        database2
            .CreateContainerIfNotExistsAsync(containerProperties: null, throughput: secondOptions.DatabaseThroughput, requestOptions: null, CancellationToken.None)
            .ReturnsForAnyArgs(containerResponse1);

        containerRegistry
            .DefaultOptions
            .Returns(options);

        containerRegistry
            .Options
            .Returns(new[] { options, secondOptions });
    }

    [Theory, AutoNSubstituteData]
    public async Task Should_Initialize_Database(
        ICosmosContainerInitializer initializer,
        CancellationToken cancellationToken)
    {
        // Arrange
        var sut = new CosmosInitializer(
            clientProvider,
            new[] { new ScopedCosmosContainerInitializer(null, initializer) },
            containerRegistry);

        // Act
        await sut.InitializeAsync(cancellationToken);

        // Assert
        _ = client
            .Received(1)
            .CreateDatabaseIfNotExistsAsync(
                options.DatabaseName,
                options.DatabaseThroughput,
                null,
                cancellationToken);
    }

    [Theory, AutoNSubstituteData]
    public async Task Should_Initialize_Database_Once(
        ICosmosContainerInitializer initializer,
        CancellationToken cancellationToken)
    {
        // Arrange
        var sut = new CosmosInitializer(
            clientProvider,
            new[] { new ScopedCosmosContainerInitializer(null, initializer), new ScopedCosmosContainerInitializer(null, initializer) },
            containerRegistry);

        // Act
        await sut.InitializeAsync(cancellationToken);

        // Assert
        _ = client
            .Received(1)
            .CreateDatabaseIfNotExistsAsync(
                options.DatabaseName,
                options.DatabaseThroughput,
                null,
                cancellationToken);
    }

    [Theory, AutoNSubstituteData]
    public async Task Should_Initialize_Database_ForEach_Options(
        ICosmosContainerInitializer initializer,
        CancellationToken cancellationToken)
    {
        // Arrange
        var sut = new CosmosInitializer(
            clientProvider,
            new[] { new ScopedCosmosContainerInitializer(null, initializer), new ScopedCosmosContainerInitializer(secondOptions, initializer) },
            containerRegistry);

        // Act
        await sut.InitializeAsync(cancellationToken);

        // Assert
        _ = client
            .Received(1)
            .CreateDatabaseIfNotExistsAsync(
                options.DatabaseName,
                options.DatabaseThroughput,
                null,
                cancellationToken);

        _ = client2
            .Received(1)
            .CreateDatabaseIfNotExistsAsync(
                secondOptions.DatabaseName,
                secondOptions.DatabaseThroughput,
                null,
                cancellationToken);
    }

    [Theory, AutoNSubstituteData]
    public async Task Should_Initialize_Initializers(
        [Substitute] ICosmosContainerInitializer initializer,
        CancellationToken cancellationToken)
    {
        // Arrange
        var sut = new CosmosInitializer(
            clientProvider,
            new[] { new ScopedCosmosContainerInitializer(null, initializer) },
            containerRegistry);

        // Act
        await sut.InitializeAsync(cancellationToken);

        // Assert
        _ = initializer
            .Received(1)
            .InitializeAsync(
                database,
                cancellationToken);
    }

    [Theory, AutoNSubstituteData]
    public async Task Should_Initialize_Initializers_By_Options(
        [Substitute] ICosmosContainerInitializer initializer,
        CancellationToken cancellationToken)
    {
        // Arrange
        var sut = new CosmosInitializer(
            clientProvider,
            new[] { new ScopedCosmosContainerInitializer(null, initializer), new ScopedCosmosContainerInitializer(secondOptions, initializer) },
            containerRegistry);

        // Act
        await sut.InitializeAsync(cancellationToken);

        // Assert
        _ = initializer
            .Received(1)
            .InitializeAsync(
                database,
                cancellationToken);

        _ = initializer
            .Received(1)
            .InitializeAsync(
                database2,
                cancellationToken);
    }

    [Theory, AutoNSubstituteData]
    public async Task Should_Initialize_Database_Only_For_Scoped_Options(
        ICosmosContainerInitializer initializer,
        CancellationToken cancellationToken)
    {
        // Arrange
        containerRegistry
            .Options
            .Returns(new[] { options, secondOptions });

        var sut = new CosmosInitializer(
            clientProvider,
            new[] { new ScopedCosmosContainerInitializer(secondOptions, initializer) },
            containerRegistry);

        // Act
        await sut.InitializeAsync(cancellationToken);

        // Assert
        _ = client2
            .Received(1)
            .CreateDatabaseIfNotExistsAsync(
                secondOptions.DatabaseName,
                secondOptions.DatabaseThroughput,
                null,
                cancellationToken);
    }

    [Theory, AutoNSubstituteData]
    public async Task Should_Initialize_Initializers_Only_For_Scoped_Options(
        [Substitute] ICosmosContainerInitializer initializer,
        CancellationToken cancellationToken)
    {
        // Arrange
        containerRegistry
            .Options
            .Returns(new[] { options, secondOptions });

        var sut = new CosmosInitializer(
            clientProvider,
            new[] { new ScopedCosmosContainerInitializer(secondOptions, initializer) },
            containerRegistry);

        // Act
        await sut.InitializeAsync(cancellationToken);

        // Assert
        _ = initializer
            .Received(1)
            .InitializeAsync(
                database2,
                cancellationToken);
    }

    [Theory, AutoNSubstituteData]
    public Task Throw_If_Failed_To_Connect_To_CosmosEmulator(
        ICosmosContainerInitializer initializer,
        CancellationToken cancellationToken)
    {
        // Arrange
        client.Endpoint.Returns(new Uri("https://localhost"));

        client.WhenForAnyArgs(c => { _ = c.CreateDatabaseIfNotExistsAsync(id: null, throughput: null, requestOptions: null, CancellationToken.None); })
            .Throw(new SocketException((int)SocketError.ConnectionRefused));

        var sut = new CosmosInitializer(
            clientProvider,
            new[] { new ScopedCosmosContainerInitializer(null, initializer) },
            containerRegistry);

        // Act & assert
        return new Func<Task>(() => sut.InitializeAsync(cancellationToken))
            .Should()
            .ThrowExactlyAsync<InvalidOperationException>()
            .WithMessage("Please start Cosmos DB Emulator");
    }

    [Theory, AutoNSubstituteData]
    public Task Throw_If_Failed_To_Connect_To_CosmosEmulator_Using_InnerException(
        ICosmosContainerInitializer initializer,
        string exceptionMessage,
        CancellationToken cancellationToken)
    {
        // Arrange
        client.Endpoint.Returns(new Uri("https://localhost"));

        client.WhenForAnyArgs(c => { _ = c.CreateDatabaseIfNotExistsAsync(id: null, throughput: null, requestOptions: null, CancellationToken.None); })
            .Throw(new Exception(exceptionMessage, new SocketException((int)SocketError.ConnectionRefused)));

        var sut = new CosmosInitializer(
            clientProvider,
            new[] { new ScopedCosmosContainerInitializer(null, initializer) },
            containerRegistry);

        // Act & assert
        return new Func<Task>(() => sut.InitializeAsync(cancellationToken))
            .Should()
            .ThrowExactlyAsync<InvalidOperationException>()
            .WithMessage("Please start Cosmos DB Emulator");
    }

    [Theory, AutoNSubstituteData]
    public Task Throw_If_Failed_To_Connect_To_CosmosEmulator_Using_AggregateException(
        ICosmosContainerInitializer initializer,
        string exceptionMessage,
        CancellationToken cancellationToken)
    {
        // Arrange
        client.Endpoint.Returns(new Uri("https://localhost"));

        client.WhenForAnyArgs(c => { _ = c.CreateDatabaseIfNotExistsAsync(id: null, throughput: null, requestOptions: null, CancellationToken.None); })
            .Throw(new AggregateException(
                exceptionMessage,
                new Exception(),
                new SocketException((int)SocketError.ConnectionRefused)));

        var sut = new CosmosInitializer(
            clientProvider,
            new[] { new ScopedCosmosContainerInitializer(null, initializer) },
            containerRegistry);

        // Act & assert
        return new Func<Task>(() => sut.InitializeAsync(cancellationToken))
            .Should()
            .ThrowExactlyAsync<InvalidOperationException>()
            .WithMessage("Please start Cosmos DB Emulator");
    }

    [Theory, AutoNSubstituteData]
    public Task Throw_Original_Exception_If_CosmosEmulator_Exception(
        ICosmosContainerInitializer initializer,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // Arrange
        client.Endpoint.Returns(new Uri("https://localhost"));

        client.WhenForAnyArgs(c => { _ = c.CreateDatabaseIfNotExistsAsync(id: null, throughput: null, requestOptions: null, CancellationToken.None); })
            .Throw(exception);

        var sut = new CosmosInitializer(
            clientProvider,
            new[] { new ScopedCosmosContainerInitializer(null, initializer) },
            containerRegistry);

        // Act & assert
        return new Func<Task>(() => sut.InitializeAsync(cancellationToken))
            .Should()
            .ThrowExactlyAsync<Exception>()
            .Where(e => e == exception);
    }

    [Theory, AutoNSubstituteData]
    public Task Throw_Original_Exception_If_Endpoint_Is_Not_Localhost(
        ICosmosContainerInitializer initializer,
        string exceptionMessage,
        CancellationToken cancellationToken)
    {
        // Arrange
        var exception = new Exception(exceptionMessage, new SocketException((int)SocketError.ConnectionRefused));

        client.WhenForAnyArgs(c => { _ = c.CreateDatabaseIfNotExistsAsync(id: null, throughput: null, requestOptions: null, CancellationToken.None); })
            .Throw(exception);

        var sut = new CosmosInitializer(
            clientProvider,
            new[] { new ScopedCosmosContainerInitializer(null, initializer) },
            containerRegistry);

        // Act & assert
        return new Func<Task>(() => sut.InitializeAsync(cancellationToken))
            .Should()
            .ThrowExactlyAsync<Exception>()
            .Where(e => e == exception);
    }
}