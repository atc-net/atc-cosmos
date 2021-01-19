# Atc.Cosmos

This repo contains the `Atc.Cosmos` library for configuring containers in Cosmos and providing an easy way to read and write document resources.

## Installation

The library is installed by adding the nuget package `Atc.Cosmos` to your project.

## Getting started

Once the library is added to your project, you will have access to the [`ICosmosReader<T>`](src/Atc.Cosmos/ICosmosReader.cs) and [`ICosmosWriter<T>`](src/Atc.Cosmos/ICosmosWriter.cs) interfaces, used for reading and writing Cosmos document resources. A document resource is represented by a class implementing the [`CosmosResource`](src/Atc.Cosmos/CosmosResource.cs) (or the [`ICosmosResource`](src/Atc.Cosmos/ICosmosResource.cs) interface).

To configure where each resource will be stored in Cosmos, the `ConfigureCosmos(buidler)` extension method is used on the `IServiceCollection` when setting up dependency injection (usually in a `Startup.cs` file).

This will be explained in the following sections:
* Configure Cosmos connection
* Configure containers
* Initialize containers

## Configure Cosmos connection

For configuring how the library connects to Cosmos, the library uses the `CosmosOptions` class. This includes the following settings:
| Name | Description |
|-|-|
| `AccountEndpoint` | Url to the Cosmos Account. |
| `AccountKey` | Key for Cosmos Account. |
| `DatabaseName` | Name of the Cosmos database (will be provisioned by the library). |
| `DatabaseThroughput` | The throughput provisioned for the database in measurement of Request Units per second in the Azure Cosmos DB service. |
| `SerializerOptions` | The `JsonSerializerOptions` used for the `System.Text.Json.JsonSerializer`. |

<br/>

There are 3 ways to provide the `CosmosOptions` to the library:
1. As an argument to the `ConfigureCosmos()` extension method.
2. As a `Func<IServiceProvider, CosmosOptions>` factory method argument on the `ConfigureCosmos()` extension method.
3. As a `IOptions<CosmosOptions>` instance configured using the Options framework and registered in dependency injection.

    This could be done by e.g. reading the `CosmosOptions` from configuration, like this:

    ```
    services.Configure<CosmosOptions>(
      Configuration.GetSection(configurationSectionName));
    ```

    Or by using a factory class implementing the `IConfigureOptions<CosmosOptions>` interface and register it like this:

    ```
    services.ConfigureOptions<ConfigureCosmosOptions>();
    ```

    The latter is the recommended approach.

## Configure containers

For each Cosmos resource you want to access using the `ICosmosReader<T>` and `ICosmosWriter<T>` you will need to:

1. Create class representing the Cosmos document resource.

    The class should implement the abstract `CosmosResource` base-class, which requires `GetDocumentId()` and `GetPartitionKey()` methods to be implemented.

    The class will be serialized to Cosmos using the `System.Text.Json.JsonSerializer`, so the `System.Text.Json.Serializaion.JsonPropertyNameAttribute` can be used to control the actual property name in the json document. 

    This can e.g. be usefull when referencing the name of the id and partition key properties in a `ICosmosContainerInitializer` implementation which is described further down.

2. Configure the container used for the Cosmos document resource.

    This is done on the `ICosmosBuilder` made available using the `ConfigureCosmos()` extension on the `IServiceCollection`, like this:

    ```
    public void ConfigureServices(IServiceCollection services)
    {
      services.ConfigureCosmos(b => b.AddContainer<MyResource>(containerName));
    }
    ```

## Initialize containers

The library supports adding initializers for each container, that can then be used to create
the container, and configure it with the correct keys and indexes.

To do this you will need to:

1. Create an initializer by implementing the `ICosmosContainerInitializer` interface.

    Usually the implementation will call the `CreateContainerIfNotExistsAsync()` method on
    the provided `Database` object with the desired `ContainerProperties`.

2. Setup the initializer to be run during initialization

    This is done on the `ICosmosBuilder` made available using the `ConfigureCosmos()` extension on the `IServiceCollection`, like this:

    ```
    public void ConfigureServices(IServiceCollection services)
    {
      services.ConfigureCosmos(b => b.AddContainer<MyInitializer>(containerName));
    }
    ```

3. Chose a way to run the initialization

    For an AspNet Core services, a HostedService can be used, like this:
    ```
    public void ConfigureServices(IServiceCollection services)
    {
      services.ConfigureCosmos(b => b.UseHostedService()));
    }
    ```

    For Azure Functions, the `AzureFunctionInitializeCosmosDatabase()` extension methid
    can be used to execute the initialization (synchronously) like this:
    ```
    public void Configure(IWebJobsBuilder builder)
    {
        ConfigureServices(builder.Services);
        builder.Services.AzureFunctionInitializeCosmosDatabase();
    }
    ```


