using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace Atc.Cosmos.Internal
{
    /// <summary>
    /// Responsible for initializing cosmos database and containers, including require throughput (RU/Request Units).
    /// </summary>
    public class CosmosInitializer : ICosmosInitializer
    {
        private readonly ICosmosClientProvider provider;
        private readonly ICosmosContainerRegistry registry;
        private readonly IReadOnlyList<ICosmosContainerInitializer> initializers;

        public CosmosInitializer(
            ICosmosClientProvider provider,
            IEnumerable<ICosmosContainerInitializer> initializers,
            ICosmosContainerRegistry registry)
        {
            this.provider = provider;
            this.registry = registry;
            this.initializers = initializers.ToList();
        }

        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            foreach (var option in registry.Options)
            {
                var database = await GetOrCreateDatabaseAsync(option, cancellationToken)
                    .ConfigureAwait(false);

                var initializerTasks = initializers
                    .Select(init => init.InitializeAsync(database, cancellationToken));

                await Task.WhenAll(initializerTasks)
                    .ConfigureAwait(false);
            }
        }

        private async Task<Database> GetOrCreateDatabaseAsync(CosmosOptions options, CancellationToken cancellationToken)
        {
            try
            {
                var response = await provider
                    .GetClient(options)
                    .CreateDatabaseIfNotExistsAsync(
                        options.DatabaseName,
                        options.DatabaseThroughput,
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                return response.Database;
            }
            catch (Exception ex)
             when (IsCosmosEmulatorMissing(ex, options))
            {
                throw new InvalidOperationException(
                    "Please start Cosmos DB Emulator");
            }
        }

        private bool IsCosmosEmulatorMissing(Exception ex, CosmosOptions options)
            => provider.GetClient(options).Endpoint.IsLoopback
            && IsConnectionRefused(ex, options);

        private bool IsConnectionRefused(Exception ex, CosmosOptions options) => ex switch
        {
            SocketException
            { SocketErrorCode: SocketError.ConnectionRefused }
                => true,

            AggregateException ae
            when ae.InnerExceptions.Any(e => IsCosmosEmulatorMissing(e, options))
                => true,

            Exception { InnerException: var inner }
            when inner != null
                => IsCosmosEmulatorMissing(inner, options),

            _ => false
        };
    }
}