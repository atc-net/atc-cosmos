using System;
using System.Collections.Generic;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace Atc.Cosmos.Internal
{
    public class CosmosContainerProvider : ICosmosContainerProvider
    {
        private readonly ICosmosClientProvider clientProvider;
        private readonly IEnumerable<ICosmosContainerNameProvider> nameProviders;
        private readonly CosmosOptions options;

        public CosmosContainerProvider(
            ICosmosClientProvider clientProvider,
            IOptions<CosmosOptions> options,
            IEnumerable<ICosmosContainerNameProvider> nameProviders)
        {
            this.clientProvider = clientProvider;
            this.nameProviders = nameProviders;
            this.options = options.Value;
        }

        public Container GetContainer<T>(
            bool allowBulk = false)
            => GetContainer(
                GetContainerName(typeof(T)),
                allowBulk);

        public Container GetContainer(
            Type resourceType,
            bool allowBulk = false)
            => GetContainer(
                GetContainerName(resourceType),
                allowBulk);

        public Container GetContainer(
            string name,
            bool allowBulk = false)
            => GetClient(allowBulk)
                .GetContainer(
                    options.DatabaseName,
                    name);

        private string GetContainerName(
            Type resourceType)
        {
            foreach (var provider in nameProviders)
            {
                if (provider.GetContainerName(resourceType) is { } name)
                {
                    return name;
                }
            }

            throw new NotSupportedException(
                $"Type {resourceType.Name} is not supported.");
        }

        private CosmosClient GetClient(bool allowBulk)
            => allowBulk
             ? clientProvider.GetBulkClient()
             : clientProvider.GetClient();
    }
}