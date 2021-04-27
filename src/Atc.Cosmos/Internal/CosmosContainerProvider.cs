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

        private readonly ICosmosClientProvider clientProvider;
        private readonly CosmosOptions options;

        public CosmosContainerProvider(
            ICosmosClientProvider clientProvider,
            IOptions<CosmosOptions> options,
            IEnumerable<ICosmosContainerNameProvider> nameProviders)
        {
            this.clientProvider = clientProvider;
            this.options = options.Value;
            containerNames = nameProviders.ToDictionary(p => p.FromType, p => p.ContainerName);
        }

        public Container GetContainer<T>(bool allowBulk = false)
            => GetClient(allowBulk)
                .GetContainer(
                    options.DatabaseName,
                    GetContainerName<T>());

        private string GetContainerName<T>()
            => containerNames.TryGetValue(
                typeof(T),
                out var containerName)
             ? containerName
             : throw new NotSupportedException($"Type {typeof(T)} is not supported.");

        private CosmosClient GetClient(bool allowBulk)
            => allowBulk
             ? clientProvider.GetBulkClient()
             : clientProvider.GetClient();
    }
}