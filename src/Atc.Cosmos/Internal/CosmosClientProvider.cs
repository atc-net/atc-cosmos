using System;
using System.Collections.Concurrent;
using System.Linq;
using Atc.Cosmos.Serialization;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace Atc.Cosmos.Internal
{
    public sealed class CosmosClientProvider(
        IOptions<CosmosClientOptions> cosmosClientOptions,
        IJsonCosmosSerializer serializer)
        : IDisposable, ICosmosClientProvider
    {
        private readonly ConcurrentDictionary<CosmosOptions, CosmosClient> cosmosClientCache = new ();
        private readonly ConcurrentDictionary<CosmosOptions, CosmosClient> cosmosBulkClientCache = new ();

        public CosmosClient GetClient(CosmosOptions options)
            => cosmosClientCache.AddOrUpdate(options, CreateClient, (_, c) => c);

        public CosmosClient GetBulkClient(CosmosOptions options)
            => cosmosBulkClientCache.AddOrUpdate(options, CreateBulkClient, (_, c) => c);

        public void Dispose()
        {
            foreach (var client in cosmosClientCache.ToList())
            {
                client.Value.Dispose();
            }

            foreach (var client in cosmosBulkClientCache.ToList())
            {
                client.Value.Dispose();
            }
        }

        private CosmosClient CreateBulkClient(CosmosOptions cosmosOptions) => CreateClient(cosmosOptions, true);

        private CosmosClient CreateClient(CosmosOptions cosmosOptions) => CreateClient(cosmosOptions, false);

        private CosmosClient CreateClient(CosmosOptions cosmosOptions, bool allowBulk)
        {
            var connectionString =
                $"AccountEndpoint={cosmosOptions.AccountEndpoint};" +
                $"AccountKey={cosmosOptions.AccountKey}";

            var options = CreateCosmosClientOptions();
            options.AllowBulkExecution = allowBulk;
            options.Serializer = cosmosClientOptions.Value.Serializer ?? new CosmosSerializerAdapter(serializer);
            options.CosmosClientTelemetryOptions = cosmosClientOptions.Value.CosmosClientTelemetryOptions;

            return cosmosOptions.Credential is not null
                ? new CosmosClient(
                    cosmosOptions.AccountEndpoint,
                    cosmosOptions.Credential,
                    options)
                : new CosmosClient(connectionString, options);
        }

        private CosmosClientOptions CreateCosmosClientOptions()
        {
            var result = new CosmosClientOptions();

            if (cosmosClientOptions is { Value: { } o })
            {
                if (!string.IsNullOrEmpty(o.ApplicationName))
                {
                    result.ApplicationName = o.ApplicationName;
                }

                result.ApplicationPreferredRegions = o.ApplicationPreferredRegions;
                result.ApplicationRegion = o.ApplicationRegion;
                result.ConnectionMode = o.ConnectionMode;
                result.ConsistencyLevel = o.ConsistencyLevel;

                foreach (var handler in o.CustomHandlers)
                {
                    result.CustomHandlers.Add(handler);
                }

                result.HttpClientFactory = o.HttpClientFactory;
                result.IdleTcpConnectionTimeout = o.IdleTcpConnectionTimeout;
                result.LimitToEndpoint = o.LimitToEndpoint;
                result.MaxRequestsPerTcpConnection = o.MaxRequestsPerTcpConnection;
                result.MaxRetryWaitTimeOnRateLimitedRequests = o.MaxRetryWaitTimeOnRateLimitedRequests;
                result.MaxTcpConnectionsPerEndpoint = o.MaxTcpConnectionsPerEndpoint;
                result.OpenTcpConnectionTimeout = o.OpenTcpConnectionTimeout;
                result.PortReuseMode = o.PortReuseMode;
                result.RequestTimeout = o.RequestTimeout;
                result.SerializerOptions = o.SerializerOptions;
                result.WebProxy = o.WebProxy;
                result.EnableTcpConnectionEndpointRediscovery = o.EnableTcpConnectionEndpointRediscovery;
                result.GatewayModeMaxConnectionLimit = o.GatewayModeMaxConnectionLimit;
                result.MaxRetryAttemptsOnRateLimitedRequests = o.MaxRetryAttemptsOnRateLimitedRequests;
                result.CosmosClientTelemetryOptions = o.CosmosClientTelemetryOptions;
            }

            return result;
        }
    }
}