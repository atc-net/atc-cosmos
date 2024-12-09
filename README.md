### General Project Info
[![Github top language](https://img.shields.io/github/languages/top/atc-net/atc-cosmos)](https://github.com/atc-net/atc-cosmos)
[![Github stars](https://img.shields.io/github/stars/atc-net/atc-cosmos)](https://github.com/atc-net/atc-cosmos)
[![Github forks](https://img.shields.io/github/forks/atc-net/atc-cosmos)](https://github.com/atc-net/atc-cosmos)
[![Github size](https://img.shields.io/github/repo-size/atc-net/atc-cosmos)](https://github.com/atc-net/atc-cosmos)
[![Issues Open](https://img.shields.io/github/issues/atc-net/atc-cosmos.svg?logo=github)](https://github.com/atc-net/atc-cosmos/issues)

### Packages
[![Github Version](https://img.shields.io/static/v1?logo=github&color=blue&label=github&message=latest)](https://github.com/orgs/atc-net/packages?repo_name=atc-cosmos)
[![NuGet Version](https://img.shields.io/nuget/v/Atc.Cosmos.svg?logo=nuget)](https://www.nuget.org/packages/Atc.Cosmos)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Atc.Cosmos.svg?style=flat-square&label=downloads)](http://www.nuget.org/packages/Atc.Cosmos)

### Build Status
![Pre-Integration](https://github.com/atc-net/atc-cosmos/workflows/Pre-Integration/badge.svg)
![Post-Integration](https://github.com/atc-net/atc-cosmos/workflows/Post-Integration/badge.svg)
![Release](https://github.com/atc-net/atc-cosmos/workflows/Release/badge.svg)

### Code Quality
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=atc-cosmos&metric=sqale_rating)](https://sonarcloud.io/dashboard?id=atc-cosmos)
[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=atc-cosmos&metric=reliability_rating)](https://sonarcloud.io/dashboard?id=atc-cosmos)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=atc-cosmos&metric=security_rating)](https://sonarcloud.io/dashboard?id=atc-cosmos)
[![Bugs](https://sonarcloud.io/api/project_badges/measure?project=atc-cosmos&metric=bugs)](https://sonarcloud.io/dashboard?id=atc-cosmos)
[![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=atc-cosmos&metric=vulnerabilities)](https://sonarcloud.io/dashboard?id=atc-cosmos)

# Atc.Cosmos

This repo contains the `Atc.Cosmos` library for configuring containers in Cosmos and providing an easy way to read and write document resources.

## Installation

The library is installed by adding the nuget package `Atc.Cosmos` to your project.

## Getting started

Once the library is added to your project, you will have access to the following interfaces, used for reading and writing Cosmos document resources:
* [`ICosmosReader<T>`](src/Atc.Cosmos/ICosmosReader.cs)
* [`ICosmosWriter<T>`](src/Atc.Cosmos/ICosmosWriter.cs)
* [`ICosmosBulkReader<T>`](src/Atc.Cosmos/ICosmosBulkReader.cs)
* [`ICosmosBulkWriter<T>`](src/Atc.Cosmos/ICosmosBulkWriter.cs)

A document resource is represented by a class deriving from the [`CosmosResource`](src/Atc.Cosmos/CosmosResource.cs) base-class, or by implementing the underlying [`ICosmosResource`](src/Atc.Cosmos/ICosmosResource.cs) interface directly.

To configure where each resource will be stored in Cosmos, the `ConfigureCosmos(builder)` extension method is used on the `IServiceCollection` when setting up dependency injection (usually in a `Startup.cs` file).

This will be explained in the following sections:
* Configure Cosmos connection
* Configure containers
* Initialize containers
* Using the readers and writers

## Configure Cosmos connection

For configuring how the library connects to Cosmos, the library uses the `CosmosOptions` class. This includes the following settings:
| Name | Description |
|-|-|
| `AccountEndpoint` | Url to the Cosmos Account. |
| `AccountKey` | Key for Cosmos Account. |
| `DatabaseName` | Name of the Cosmos database (will be provisioned by the library). |
| `DatabaseThroughput` | The throughput provisioned for the database in measurement of Request Units per second in the Azure Cosmos DB service. |
| `SerializerOptions` | The `JsonSerializerOptions` used for the `System.Text.Json.JsonSerializer`. |
| `Credential` | The `TokenCredential` used for accessing [cosmos with an Azure AD token](https://docs.microsoft.com/en-us/azure/cosmos-db/managed-identity-based-authentication). Please note that setting this property will ignore any value specified in `AccountKey`. |

<br/>

There are 3 ways to provide the `CosmosOptions` to the library:
1. As an argument to the `ConfigureCosmos()` extension method.
2. As a `Func<IServiceProvider, CosmosOptions>` factory method argument on the `ConfigureCosmos()` extension method.
3. As a `IOptions<CosmosOptions>` instance configured using the Options framework and registered in dependency injection.

    This could be done by e.g. reading the `CosmosOptions` from configuration, like this:

    ```c#
    services.Configure<CosmosOptions>(
      Configuration.GetSection(configurationSectionName));
    ```

    Or by using a factory class implementing the `IConfigureOptions<CosmosOptions>` interface and register it like this:

    ```c#
    services.ConfigureOptions<ConfigureCosmosOptions>();
    ```

    The latter is the recommended approach.

## Configure containers

For each Cosmos resource you want to access using the `ICosmosReader<T>` and `ICosmosWriter<T>` you will need to:

1. Create class representing the Cosmos document resource.

    The class should implement the abstract `CosmosResource` base-class, which requires `GetDocumentId()` and `GetPartitionKey()` methods to be implemented.

    The class will be serialized to Cosmos using the `System.Text.Json.JsonSerializer`, so the `System.Text.Json.Serialization.JsonPropertyNameAttribute` can be used to control the actual property name in the json document.

    This can e.g. be useful when referencing the name of the id and partition key properties in a `ICosmosContainerInitializer` implementation which is described further down.

2. Configure the container used for the Cosmos document resource.

    This is done on the `ICosmosBuilder` made available using the `ConfigureCosmos()` extension on the `IServiceCollection`, like this:

    ```c#
    public void ConfigureServices(IServiceCollection services)
    {
      services.ConfigureCosmos(b => b.AddContainer<MyResource>(containerName));
    }
    ```

3. If you want to connect to multiple databases you would need to scope your container to a new `CosmosOptions` instance in the following way:

    ```c#
    public void ConfigureServices(IServiceCollection services)
    {
      services.ConfigureCosmos(
          b => b.AddContainer<MyResource>(containerName)
                .ForDatabase(secondDbOptions)
                  .AddContainer<MySecondResource>(containerName));
    }
    ```
    The first call to AddContainer will be scoped to the default options as the passed builder 'b' is always scoped to the default options.
    The subsequent call to *ForDatabase* will return a new builder scoped for the options passed to this method and any subsequent calls to this builder will have the same scope.

## Initialize containers

The library supports adding initializers for each container, that can then be used to create
the container, and configure it with the correct keys and indexes.

To do this you will need to:

1. Create an initializer by implementing the `ICosmosContainerInitializer` interface.

    Usually the implementation will call the `CreateContainerIfNotExistsAsync()` method on
    the provided `Database` object with the desired `ContainerProperties`.

2. Setup the initializer to be run during initialization

    This is done on the `ICosmosBuilder` made available using the `ConfigureCosmos()` extension on the `IServiceCollection`, like this:

    ```c#
    public void ConfigureServices(IServiceCollection services)
    {
      services.ConfigureCosmos(b => b.AddContainer<MyInitializer>(containerName));
    }
    ```

3. Chose a way to run the initialization

    For an AspNet Core services, a HostedService can be used, like this:
    
    ```c#
    public void ConfigureServices(IServiceCollection services)
    {
      services.ConfigureCosmos(b => b.UseHostedService()));
    }
    ```

    For Azure Functions, the `AzureFunctionInitializeCosmosDatabase()` extension method
    can be used to execute the initialization (synchronously) like this:
    
    ```c#
    public void Configure(IWebJobsBuilder builder)
    {
        ConfigureServices(builder.Services);
        builder.Services.AzureFunctionInitializeCosmosDatabase();
    }
    ```

## Using the readers and writers

Once the setup is in place, the readers and writers are registered with the [Microsoft.Extensions.DependencyInjection](https://www.nuget.org/packages/Microsoft.Extensions.DependencyInjection/) container, and can be obtained via constructor injection on any service.

The registered interfaces are:

|Name|Description|
|-|-|
|[`ICosmosReader<T>`](src/Atc.Cosmos/ICosmosReader.cs)| Represents a reader that can read Cosmos resources. |
|[`ICosmosWriter<T>`](src/Atc.Cosmos/ICosmosWriter.cs)| Represents a writer that can write Cosmos resources. |
|[`ICosmosBulkReader<T>`](src/Atc.Cosmos/ICosmosBulkReader.cs)| Represents a reader that can perform bulk reads on Cosmos resources. |
|[`ICosmosBulkWriter<T>`](src/Atc.Cosmos/ICosmosBulkWriter.cs)| Represents a writer that can perform bulk operations on Cosmos resources. |

The bulk reader and writer are for optimizing performance when executing many operations towards Cosmos. It works by creating all the tasks and then use the `Task.WhenAll()` to await them. This will group operations by partition key and send them in batches of 100.

When not operating with bulks, the normal readers are faster as there is no delay waiting for more work.

## Change Feeds

The library supports adding change feed processors for a container.

To do this you will need to:

1. Create a processor by implementing the `IChangeFeedProcessor` interface.

2. Setup the change feed processor during initialization

    This is done on the `ICosmosBuilder<T>` made available using the `ConfigureCosmos()` extension on the `IServiceCollection`, like this:

    ```c#
    public void ConfigureServices(IServiceCollection services)
    {
      services.ConfigureCosmos(b => b
        .AddContainer<MyInitializer, MyResource>(containerName)
        .WithChangeFeedProcessor<MyChangeFeedProcessor>());
    }
    ```

    or using the `ICosmosContainerBuilder<T>` like this:

    ```c#
    public void ConfigureServices(IServiceCollection services)
    {
      services.ConfigureCosmos(b => b
        .AddContainer<MyInitializer>(
          containerName,
          c => c
            .AddResource<MyResource>()
            .WithChangeFeedProcessor<MyChangeFeedProcessor>()));
    }
    ```

*Note: The change feed processor relies on a HostedService, which means that this feature is only available in AspNet Core services.*

## Preview Features

The library also has a preview version that exposes some of CosmosDB preview features.

### Priority Based Execution

When using the preview version, you will have access to the following interfaces, used for reading and writing Cosmos document resources:

|Name|Description|
|-|-|
|[`ILowPriorityCosmosReader<T>`](src/Atc.Cosmos/ILowPriorityCosmosReader.cs)| Represents a reader that can read Cosmos resources with low priority. |
|[`ILowPriorityCosmosWriter<T>`](src/Atc.Cosmos/ILowPriorityCosmosWriter.cs)| Represents a writer that can write Cosmos resources with low priority. |
|[`ILowPriorityCosmosBulkReader<T>`](src/Atc.Cosmos/ILowPriorityCosmosBulkReader.cs)| Represents a reader that can perform bulk reads on Cosmos resources with low priority. |
|[`ILowPriorityCosmosBulkWriter<T>`](src/Atc.Cosmos/ILowPriorityCosmosBulkWriter.cs)| Represents a writer that can perform bulk operations on Cosmos resources with low priority. |

In order to use these interfaces the "Priority Based Execution" feature needs to be enabled on the CosmosDB account.

This can be done by either enabling it directly in Azure Portal under Settings -> Features tab on the CosmosDB resource.

Alternatively through Azure CLI:

```bash
# install cosmosdb-preview Azure CLI extension
az extension add --name cosmosdb-preview

# Enable priority-based execution
az cosmosdb update  --resource-group $ResourceGroup --name $AccountName --enable-priority-based-execution true
```

See [MS Learn](https://learn.microsoft.com/en-us/azure/cosmos-db/priority-based-execution) for more details.

## Unit Testing
The reader and writer interfaces can easily be mocked, but in some cases it is nice to have a fake version of a reader or writer to mimic the behavior of the read and write operations. For this purpose the `Atc.Cosmos.Testing` namespace contains the following fakes:

|Name|Description|
|-|-|
|[`FakeCosmosReader<T>`](src/Atc.Cosmos/Testing/FakeCosmosReader.cs)| Used for faking an [`ICosmosReader<T>`](src/Atc.Cosmos/ICosmosReader.cs) or [`ICosmosBulkReader<T>`](src/Atc.Cosmos/ICosmosBulkReader.cs). |
|[`FakeCosmosWriter<T>`](src/Atc.Cosmos/Testing/FakeCosmosWriter.cs)| Used for faking an [`ICosmosWriter<T>`](src/Atc.Cosmos/ICosmosWriter.cs) or [`ICosmosBulkWriter<T>`](src/Atc.Cosmos/ICosmosBulkWriter.cs). |
|[`FakeCosmos<T>`](src/Atc.Cosmos/Testing/FakeCosmos.cs)| Used for getting a `FakeCosmosReader` and `FakeCosmosWriter` that share state. |

Using the [Atc.Test](https://github.com/atc-net/atc-test) setup a test using the fakes could look like this:

```csharp
[Theory, AutoNSubstituteData]
public async Task Should_Update_Cosmos_With_NewData(
    [Frozen(Matching.ImplementedInterfaces)]
    FakeCosmos<MyCosmosResource> cosmos,
    MyCosmosService sut,
    MyCosmosResource resource,
    string newData)
{
    cosmos.Documents.Add(resource);

    await service.UpdateAsync(resource.Id, newData);

    resource
        .Data
        .Should()
        .Be(newData);
}
```
