using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace Atc.Cosmos.Internal
{
    public class CosmosContainerProvider : ICosmosContainerProvider
    {
        private readonly Dictionary<Type, string> containerNames;

        private readonly CosmosClient client;
        private readonly CosmosOptions options;

        public CosmosContainerProvider(
            CosmosClient client,
            IOptions<CosmosOptions> options,
            IEnumerable<ICosmosContainerNameProvider> providers)
        {
            this.client = client;
            this.options = options.Value;
            containerNames = providers.ToDictionary(p => p.FromType, p => p.ContainerName);
        }

        public Container GetContainer<T>()
            => containerNames.TryGetValue(typeof(T), out var containerName)
            ? client.GetContainer(options.DatabaseName, containerName)
            : throw new NotSupportedException($"Type {typeof(T)} is not supported.");
    }
}