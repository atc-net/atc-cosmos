using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Atc.Cosmos.Internal;
using Atc.Test;
using AutoFixture.AutoNSubstitute;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace Atc.Cosmos.Tests.Internal
{
    public class CosmosInitializerTests
    {
        private readonly ICosmosClientProvider clientProvider;
        private readonly CosmosClient client;
        private readonly Database database;
        private readonly DatabaseResponse databaseResponse;
        private readonly ContainerResponse containerResponse;
        private readonly ICosmosDatabaseNameProvider databaseNameProvider;
        private readonly CosmosOptions options;

        public CosmosInitializerTests()
        {
            clientProvider = Substitute.For<ICosmosClientProvider>();
            client = Substitute.For<CosmosClient>();
            database = Substitute.For<Database>();
            databaseResponse = Substitute.For<DatabaseResponse>();
            containerResponse = Substitute.For<ContainerResponse>();
            databaseNameProvider = Substitute.For<ICosmosDatabaseNameProvider>();
            options = Substitute.For<CosmosOptions>();

            clientProvider
                .GetClient()
                .Returns(client);

            client
                .CreateDatabaseIfNotExistsAsync("default", throughput: default, default, default)
                .ReturnsForAnyArgs(databaseResponse);

            databaseResponse
                .Database
                .Returns(database);

            database
                .CreateContainerIfNotExistsAsync(default, throughput: default, default, default)
                .ReturnsForAnyArgs(containerResponse);

            databaseNameProvider
                .DatabaseNames
                .Returns(new string[] { options.DatabaseName });
        }

        [Theory, AutoNSubstituteData]
        public async Task Should_Initialize_Database(
            ICosmosContainerInitializer initializer,
            CancellationToken cancellationToken)
        {
            var sut = new CosmosInitializer(clientProvider, new OptionsWrapper<CosmosOptions>(options), new[] { initializer }, databaseNameProvider);

            await sut.InitializeAsync(cancellationToken);

            _ = client
                .Received(1)
                .CreateDatabaseIfNotExistsAsync(
                    options.DatabaseName,
                    options.DatabaseThroughput,
                    null,
                    cancellationToken);
        }

        [Theory, AutoNSubstituteData]
        public async Task Should_Initialize_Initializers(
            [Substitute] ICosmosContainerInitializer initializer,
            CancellationToken cancellationToken)
        {
            var sut = new CosmosInitializer(clientProvider, new OptionsWrapper<CosmosOptions>(options), new[] { initializer }, databaseNameProvider);

            await sut.InitializeAsync(cancellationToken);

            _ = initializer
                .Received(1)
                .InitializeAsync(
                    database,
                    cancellationToken);
        }

        [Theory, AutoNSubstituteData]
        public void Throw_If_Failed_To_Connect_To_CosmosEmulator(
            ICosmosContainerInitializer initializer,
            CancellationToken cancellationToken)
        {
            client.Endpoint.Returns(new Uri("https://localhost"));
            client.WhenForAnyArgs(c => c.CreateDatabaseIfNotExistsAsync(default, throughput: default, default, default))
                .Throw(new SocketException((int)SocketError.ConnectionRefused));

            var sut = new CosmosInitializer(clientProvider, new OptionsWrapper<CosmosOptions>(options), new[] { initializer }, databaseNameProvider);
            new Func<Task>(() => sut.InitializeAsync(cancellationToken))
                .Should()
                .ThrowExactlyAsync<InvalidOperationException>()
                .WithMessage("Please start Cosmos DB Emulator");
        }

        [Theory, AutoNSubstituteData]
        public void Throw_If_Failed_To_Connect_To_CosmosEmulator_Using_InnerException(
            ICosmosContainerInitializer initializer,
            string exceptionMessage,
            CancellationToken cancellationToken)
        {
            client.Endpoint.Returns(new Uri("https://localhost"));
            client.WhenForAnyArgs(c => c.CreateDatabaseIfNotExistsAsync(default, throughput: default, default, default))
                .Throw(new Exception(exceptionMessage, new SocketException((int)SocketError.ConnectionRefused)));

            var sut = new CosmosInitializer(clientProvider, new OptionsWrapper<CosmosOptions>(options), new[] { initializer }, databaseNameProvider);
            new Func<Task>(() => sut.InitializeAsync(cancellationToken))
                .Should()
                .ThrowExactlyAsync<InvalidOperationException>()
                .WithMessage("Please start Cosmos DB Emulator");
        }

        [Theory, AutoNSubstituteData]
        public void Throw_If_Failed_To_Connect_To_CosmosEmulator_Using_AggregateException(
            ICosmosContainerInitializer initializer,
            string exceptionMessage,
            CancellationToken cancellationToken)
        {
            client.Endpoint.Returns(new Uri("https://localhost"));
            client.WhenForAnyArgs(c => c.CreateDatabaseIfNotExistsAsync(default, throughput: default, default, default))
                .Throw(new AggregateException(
                    exceptionMessage,
                    new Exception(),
                    new SocketException((int)SocketError.ConnectionRefused)));

            var sut = new CosmosInitializer(clientProvider, new OptionsWrapper<CosmosOptions>(options), new[] { initializer }, databaseNameProvider);
            new Func<Task>(() => sut.InitializeAsync(cancellationToken))
                .Should()
                .ThrowExactlyAsync<InvalidOperationException>()
                .WithMessage("Please start Cosmos DB Emulator");
        }

        [Theory, AutoNSubstituteData]
        public void Throw_Original_Exception_If_CosmosEmulator_Exception(
            ICosmosContainerInitializer initializer,
            Exception exception,
            CancellationToken cancellationToken)
        {
            client.Endpoint.Returns(new Uri("https://localhost"));
            client.WhenForAnyArgs(c => c.CreateDatabaseIfNotExistsAsync(default, throughput: default, default, default))
                .Throw(exception);

            var sut = new CosmosInitializer(clientProvider, new OptionsWrapper<CosmosOptions>(options), new[] { initializer }, databaseNameProvider);
            new Func<Task>(() => sut.InitializeAsync(cancellationToken))
                .Should()
                .ThrowExactlyAsync<Exception>()
                .Where(e => e == exception);
        }

        [Theory, AutoNSubstituteData]
        public void Throw_Original_Exception_If_Endpoint_Is_Not_Localhost(
            ICosmosContainerInitializer initializer,
            string exceptionMessage,
            CancellationToken cancellationToken)
        {
            var exception = new Exception(exceptionMessage, new SocketException((int)SocketError.ConnectionRefused));
            client.WhenForAnyArgs(c => c.CreateDatabaseIfNotExistsAsync(default, throughput: default, default, default))
                .Throw(exception);

            var sut = new CosmosInitializer(clientProvider, new OptionsWrapper<CosmosOptions>(options), new[] { initializer }, databaseNameProvider);
            new Func<Task>(() => sut.InitializeAsync(cancellationToken))
                .Should()
                .ThrowExactlyAsync<Exception>()
                .Where(e => e == exception);
        }
    }
}