using System;
using Atc.Cosmos.Serialization;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace Atc.Cosmos.Internal
{
    public sealed class CosmosClientProvider : IDisposable, ICosmosClientProvider
    {
        private readonly IOptions<CosmosOptions> cosmosOptions;
        private readonly IOptions<CosmosClientOptions> cosmosClientOptions;
        private readonly IJsonCosmosSerializer serializer;
        private CosmosClient? client;
        private CosmosClient? bulkClient;

        public CosmosClientProvider(
            IOptions<CosmosOptions> cosmosOptions,
            IOptions<CosmosClientOptions> cosmosClientOptions,
            IJsonCosmosSerializer serializer)
        {
            this.cosmosOptions = cosmosOptions;
            this.cosmosClientOptions = cosmosClientOptions;
            this.serializer = serializer;

            var options = cosmosOptions.Value;
            if (!IsValid(options))
            {
                throw new InvalidOperationException(
                    $"Invalid configuration in {nameof(CosmosOptions)}.");
            }
        }

        public CosmosClient GetClient()
            => client ??= CreateClient(allowBulk: false);

        public CosmosClient GetBulkClient()
            => bulkClient ??= CreateClient(allowBulk: true);

        public void Dispose()
        {
            client?.Dispose();
            bulkClient?.Dispose();
        }

        private static bool IsValid(CosmosOptions? options)
            => options is not null
            && !string.IsNullOrEmpty(options.AccountEndpoint)
            && (!string.IsNullOrEmpty(options.AccountKey) || options.Credential is not null)
            && !string.IsNullOrEmpty(options.DatabaseName);

        private CosmosClient CreateClient(bool allowBulk)
        {
            var connectionString =
                $"AccountEndpoint={cosmosOptions.Value.AccountEndpoint};" +
                $"AccountKey={cosmosOptions.Value.AccountKey}";

            var options = CreateCosmosClientOptions();
            options.AllowBulkExecution = allowBulk;
            options.Serializer = cosmosClientOptions.Value.Serializer
                ?? new CosmosSerializerAdapter(serializer);

            return cosmosOptions.Value.Credential is not null
                 ? new CosmosClient(
                     cosmosOptions.Value.AccountEndpoint,
                     cosmosOptions.Value.Credential,
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