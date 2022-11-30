using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Atc.Cosmos.Serialization;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace Atc.Cosmos.Internal
{
    public sealed class CosmosClientProvider : IDisposable, ICosmosClientProvider
    {
        private readonly IOptions<CosmosClientOptions> cosmosClientOptions;
        private readonly IJsonCosmosSerializer serializer;
        private readonly ConcurrentDictionary<CosmosOptions, CosmosClient> cosmosClientCache;
        private readonly ConcurrentDictionary<CosmosOptions, CosmosClient> cosmosBulkClientCache;

        public CosmosClientProvider(
            IOptions<CosmosClientOptions> cosmosClientOptions,
            IJsonCosmosSerializer serializer)
        {
            this.cosmosClientOptions = cosmosClientOptions;
            this.serializer = serializer;
            cosmosClientCache = new ConcurrentDictionary<CosmosOptions, CosmosClient>();
            cosmosBulkClientCache = new ConcurrentDictionary<CosmosOptions, CosmosClient>();
        }

        public CosmosClient GetClient(CosmosOptions options)
            => cosmosClientCache.AddOrUpdate(options, CreateClient(options, allowBulk: false), (o, c) => c);

        public CosmosClient GetBulkClient(CosmosOptions options)
            => cosmosBulkClientCache.AddOrUpdate(options, CreateClient(options, allowBulk: true), (o, c) => c);

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

        private CosmosClient CreateClient(CosmosOptions cosmosOptions, bool allowBulk)
        {
            var connectionString =
                $"AccountEndpoint={cosmosOptions.AccountEndpoint};" +
                $"AccountKey={cosmosOptions.AccountKey}";

            var options = CreateCosmosClientOptions();
            options.AllowBulkExecution = allowBulk;
            options.Serializer = cosmosClientOptions.Value.Serializer
                ?? new CosmosSerializerAdapter(serializer);

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
                result.ApplicationName = o.ApplicationName;
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
            }

            return result;
        }
    }
}