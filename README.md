# Atc.Cosmos

A .NET library for configuring containers in Azure Cosmos DB and providing an easy way to read and write document resources using `System.Text.Json`.

[![NuGet Version](https://img.shields.io/nuget/v/Atc.Cosmos.svg?logo=nuget)](https://www.nuget.org/packages/Atc.Cosmos)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Atc.Cosmos.svg?style=flat-square&label=downloads)](https://www.nuget.org/packages/Atc.Cosmos)

## Table of Contents

- [Atc.Cosmos](#atccosmos)
  - [Features](#features)
  - [Installation](#installation)
  - [Getting Started](#getting-started)
  - [Configure Cosmos Connection](#configure-cosmos-connection)
  - [Configure Containers](#configure-containers)
  - [Initialize Containers](#initialize-containers)
  - [Using the Readers and Writers](#using-the-readers-and-writers)
  - [Change Feeds](#change-feeds)
  - [Delete by Partition Key](#delete-by-partition-key)
  - [Priority Based Execution](#priority-based-execution)
  - [Unit Testing](#unit-testing)
  - [Sample](#sample)
  - [Requirements](#requirements)
  - [How to contribute](#how-to-contribute)

## Features

- **Container configuration** — declarative setup of Cosmos DB containers with automatic provisioning
- **Read/Write abstractions** — `ICosmosReader<T>` and `ICosmosWriter<T>` for simple CRUD operations
- **Bulk operations** — `ICosmosBulkReader<T>` and `ICosmosBulkWriter<T>` for high-throughput batch operations
- **Change feed processing** — built-in change feed processor support with partitioned data handling
- **Paged queries** — efficient pagination with continuation token support
- **LINQ queries** — query builder support using `IQueryable<T>`
- **Cross-partition queries** — read and query across partition boundaries
- **Optimistic concurrency** — ETag-based conflict detection on write operations
- **Update pattern** — read-modify-write with automatic retry on conflicts
- **Delete by partition key** — bulk delete all documents in a partition
- **Multi-database support** — connect to multiple Cosmos DB databases from a single application
- **Unit testing fakes** — `FakeCosmos<T>` for testing without a real Cosmos DB instance
- **System.Text.Json** — serialization using `System.Text.Json` with configurable `JsonSerializerOptions`

## Installation

```bash
dotnet add package Atc.Cosmos
```

## Getting Started

Once the library is added to your project, you will have access to the following interfaces for reading and writing Cosmos document resources:

| Interface | Description |
|-----------|-------------|
| [`ICosmosReader<T>`](src/Atc.Cosmos/ICosmosReader.cs) | Read Cosmos resources |
| [`ICosmosWriter<T>`](src/Atc.Cosmos/ICosmosWriter.cs) | Write Cosmos resources |
| [`ICosmosBulkReader<T>`](src/Atc.Cosmos/ICosmosBulkReader.cs) | Bulk read operations |
| [`ICosmosBulkWriter<T>`](src/Atc.Cosmos/ICosmosBulkWriter.cs) | Bulk write operations |

A document resource is represented by a class deriving from the [`CosmosResource`](src/Atc.Cosmos/CosmosResource.cs) base class, or by implementing the [`ICosmosResource`](src/Atc.Cosmos/ICosmosResource.cs) interface directly.

To configure where each resource will be stored in Cosmos, the `ConfigureCosmos(builder)` extension method is used on the `IServiceCollection` when setting up dependency injection.

## Configure Cosmos Connection

The library uses the `CosmosOptions` class for configuring the connection to Cosmos:

| Name | Description |
|------|-------------|
| `AccountEndpoint` | URL to the Cosmos Account |
| `AccountKey` | Key for the Cosmos Account |
| `DatabaseName` | Name of the Cosmos database (will be provisioned by the library) |
| `DatabaseThroughput` | Throughput provisioned for the database in Request Units per second |
| `SerializerOptions` | `JsonSerializerOptions` used for `System.Text.Json.JsonSerializer` |
| `Credential` | `TokenCredential` for [Azure AD authentication](https://docs.microsoft.com/en-us/azure/cosmos-db/managed-identity-based-authentication). When set, `AccountKey` is ignored |

There are 3 ways to provide the `CosmosOptions` to the library:

1. As an argument to the `ConfigureCosmos()` extension method.
2. As a `Func<IServiceProvider, CosmosOptions>` factory method argument on the `ConfigureCosmos()` extension method.
3. As an `IOptions<CosmosOptions>` instance configured using the Options framework and registered in dependency injection.

    This could be done by reading the `CosmosOptions` from configuration:

    ```csharp
    services.Configure<CosmosOptions>(
      Configuration.GetSection(configurationSectionName));
    ```

    Or by using a factory class implementing the `IConfigureOptions<CosmosOptions>` interface:

    ```csharp
    services.ConfigureOptions<ConfigureCosmosOptions>();
    ```

    The latter is the recommended approach.

## Configure Containers

For each Cosmos resource you want to access using the readers and writers you will need to:

1. **Create a class** representing the Cosmos document resource.

    The class should implement the abstract `CosmosResource` base class, which requires `GetDocumentId()` and `GetPartitionKey()` methods to be implemented.

    The class will be serialized using `System.Text.Json.JsonSerializer`, so `JsonPropertyNameAttribute` can be used to control property names in the JSON document.

2. **Configure the container** used for the Cosmos document resource.

    This is done on the `ICosmosBuilder` made available using the `ConfigureCosmos()` extension on `IServiceCollection`:

    ```csharp
    builder.Services.ConfigureCosmos(b => b.AddContainer<MyResource>(containerName));
    ```

3. **Connect to multiple databases** by scoping your container to a new `CosmosOptions` instance:

    ```csharp
    builder.Services.ConfigureCosmos(
        b => b.AddContainer<MyResource>(containerName)
              .ForDatabase(secondDbOptions)
                .AddContainer<MySecondResource>(containerName));
    ```

    The first call to `AddContainer` is scoped to the default options. The call to `ForDatabase` returns a new builder scoped to the provided options.

## Initialize Containers

The library supports adding initializers for each container that can create the container and configure it with the correct keys and indexes.

1. **Create an initializer** by implementing the `ICosmosContainerInitializer` interface.

    Usually the implementation will call `CreateContainerIfNotExistsAsync()` on the provided `Database` object with the desired `ContainerProperties`.

2. **Register the initializer** on the `ICosmosBuilder`:

    ```csharp
    builder.Services.ConfigureCosmos(b => b.AddContainer<MyInitializer>(containerName));
    ```

3. **Run the initialization** using a hosted service:

    ```csharp
    builder.Services.ConfigureCosmos(b => b.UseHostedService());
    ```

## Using the Readers and Writers

Once the setup is in place, the readers and writers are registered with the [Microsoft.Extensions.DependencyInjection](https://www.nuget.org/packages/Microsoft.Extensions.DependencyInjection/) container and can be obtained via constructor injection.

The bulk reader and writer optimize performance when executing many operations towards Cosmos. They work by creating all the tasks and then using `Task.WhenAll()` to await them, grouping operations by partition key and sending them in batches of 100.

When not operating with bulks, the normal readers are faster as there is no delay waiting for more work.

## Change Feeds

The library supports adding change feed processors for a container.

1. **Create a processor** by implementing the `IChangeFeedProcessor` interface.

2. **Register the change feed processor** during initialization:

    ```csharp
    builder.Services.ConfigureCosmos(b => b
        .AddContainer<MyInitializer, MyResource>(containerName)
        .WithChangeFeedProcessor<MyChangeFeedProcessor>());
    ```

    Or using the `ICosmosContainerBuilder<T>`:

    ```csharp
    builder.Services.ConfigureCosmos(b => b
        .AddContainer<MyInitializer>(
          containerName,
          c => c
            .AddResource<MyResource>()
            .WithChangeFeedProcessor<MyChangeFeedProcessor>()));
    ```

> **Note:** The change feed processor relies on a HostedService, which means this feature is only available in hosted applications.

## Delete by Partition Key

The `ICosmosWriter<T>.DeletePartitionAsync()` method allows you to delete all documents within a partition by partition key. This uses the Cosmos DB [delete all items by partition key](https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/how-to-delete-by-partition-key) feature.

## Priority Based Execution

The library exposes low priority readers and writers:

| Interface | Description |
|-----------|-------------|
| [`ILowPriorityCosmosReader<T>`](src/Atc.Cosmos/ILowPriorityCosmosReader.cs) | Read Cosmos resources with low priority |
| [`ILowPriorityCosmosWriter<T>`](src/Atc.Cosmos/ILowPriorityCosmosWriter.cs) | Write Cosmos resources with low priority |
| [`ILowPriorityCosmosBulkReader<T>`](src/Atc.Cosmos/ILowPriorityCosmosBulkReader.cs) | Bulk read with low priority |
| [`ILowPriorityCosmosBulkWriter<T>`](src/Atc.Cosmos/ILowPriorityCosmosBulkWriter.cs) | Bulk write with low priority |

The "Priority Based Execution" feature needs to be enabled on the CosmosDB account, either in the Azure Portal under Settings > Features, or via Azure CLI:

```bash
az cosmosdb update --resource-group $ResourceGroup --name $AccountName --enable-priority-based-execution true
```

See [Microsoft Learn](https://learn.microsoft.com/en-us/azure/cosmos-db/priority-based-execution) for more details.

## Unit Testing

The reader and writer interfaces can easily be mocked, but in some cases it is useful to have a fake implementation that mimics the behavior of read and write operations. The `Atc.Cosmos.Testing` namespace provides:

| Class | Description |
|-------|-------------|
| [`FakeCosmosReader<T>`](src/Atc.Cosmos/Testing/FakeCosmosReader.cs) | Fake `ICosmosReader<T>` / `ICosmosBulkReader<T>` |
| [`FakeCosmosWriter<T>`](src/Atc.Cosmos/Testing/FakeCosmosWriter.cs) | Fake `ICosmosWriter<T>` / `ICosmosBulkWriter<T>` |
| [`FakeCosmos<T>`](src/Atc.Cosmos/Testing/FakeCosmos.cs) | Combined fake reader and writer with shared state |

Using [Atc.Test](https://github.com/atc-net/atc-test), a test using the fakes could look like this:

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

    await sut.UpdateAsync(resource.Id, newData);

    resource
        .Data
        .Should()
        .Be(newData);
}
```

## Sample

See the [sample API](sample/SampleApi/) for an example of how to configure the library with a minimal API, including container initialization, reading, and writing resources.

## Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (or later)

## How to contribute

[Contribution Guidelines](https://atc-net.github.io/introduction/about-atc#how-to-contribute)

[Coding Guidelines](https://atc-net.github.io/introduction/about-atc#coding-guidelines)