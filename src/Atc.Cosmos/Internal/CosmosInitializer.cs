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
        private readonly ICosmosDatabaseNameProvider databaseNameProvider;
        private readonly CosmosOptions options;
        private readonly IReadOnlyList<ICosmosContainerInitializer> initializers;

        public CosmosInitializer(
            ICosmosClientProvider provider,
            IOptions<CosmosOptions> options,
            IEnumerable<ICosmosContainerInitializer> initializers,
            ICosmosDatabaseNameProvider databaseNameProvider)
        {
            this.provider = provider;
            this.databaseNameProvider = databaseNameProvider;
            this.options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            this.initializers = initializers.ToList();
        }

        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            foreach (var databaseName in databaseNameProvider.DatabaseNames)
            {
                var database = await GetOrCreateDatabaseAsync(databaseName, cancellationToken)
                    .ConfigureAwait(false);

                var initializerTasks = initializers
                    .Select(init => init.InitializeAsync(database, cancellationToken));

                await Task.WhenAll(initializerTasks)
                    .ConfigureAwait(false);
            }
        }

        private async Task<Database> GetOrCreateDatabaseAsync(string databaseName, CancellationToken cancellationToken)
        {
            try
            {
                var response = await provider
                    .GetClient()
                    .CreateDatabaseIfNotExistsAsync(
                        databaseName,
                        options.DatabaseThroughput,
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                return response.Database;
            }
            catch (Exception ex)
             when (IsCosmosEmulatorMissing(ex))
            {
                throw new InvalidOperationException(
                    "Please start Cosmos DB Emulator");
            }
        }

        private bool IsCosmosEmulatorMissing(Exception ex)
            => provider.GetClient().Endpoint.IsLoopback
            && IsConnectionRefused(ex);

        private bool IsConnectionRefused(Exception ex) => ex switch
        {
            SocketException
            { SocketErrorCode: SocketError.ConnectionRefused }
                => true,

            AggregateException ae
            when ae.InnerExceptions.Any(IsCosmosEmulatorMissing)
                => true,

            Exception { InnerException: var inner }
            when inner != null
                => IsCosmosEmulatorMissing(inner),

            _ => false
        };
    }
}