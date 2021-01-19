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
        private readonly CosmosClient client;
        private readonly CosmosOptions options;
        private readonly IReadOnlyList<ICosmosContainerInitializer> initializers;

        public CosmosInitializer(
            CosmosClient client,
            IOptions<CosmosOptions> options,
            IEnumerable<ICosmosContainerInitializer> initializers)
        {
            this.client = client;
            this.options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            this.initializers = initializers.ToList();
        }

        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            var database = await GetOrCreateDatabaseAsync(cancellationToken);

            var initializerTasks = initializers
                .Select(init => init.InitializeAsync(database, cancellationToken));

            await Task.WhenAll(initializerTasks);
        }

        private async Task<Database> GetOrCreateDatabaseAsync(CancellationToken cancellationToken)
        {
            try
            {
                var response = await client.CreateDatabaseIfNotExistsAsync(
                    options.DatabaseName,
                    options.DatabaseThroughput,
                    cancellationToken: cancellationToken);

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
            => client.Endpoint.IsLoopback
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