using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace Atc.Cosmos.Internal
{
    /// <summary>
    /// Responsible for initializing cosmos database and containers, including require throughput (RU/Request Units).
    /// </summary>
    public class CosmosInitializer : ICosmosInitializer
    {
        private readonly ICosmosClientProvider provider;
        private readonly IReadOnlyDictionary<CosmosOptions, List<ICosmosContainerInitializer>> initializers;

        public CosmosInitializer(
            ICosmosClientProvider provider,
            IEnumerable<IScopedCosmosContainerInitializer> initializers,
            ICosmosContainerRegistry registry)
        {
            this.provider = provider;
            this.initializers = initializers
                .GroupBy(i => i.Scope ?? registry.DefaultOptions)
                .ToDictionary(i => i.Key, i => i.Select(n => n.Initializer).ToList());
        }

        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            foreach (var item in initializers)
            {
                var database = await GetOrCreateDatabaseAsync(item.Key, cancellationToken)
                    .ConfigureAwait(false);

                var initializerTasks = item
                    .Value
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