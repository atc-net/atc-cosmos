using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace Atc.Cosmos.Internal
{
    public class CosmosContainerProvider : ICosmosContainerProvider
    {
        private readonly ICosmosClientProvider clientProvider;
        private readonly ICosmosContainerRegistry registry;

        public CosmosContainerProvider(
            ICosmosClientProvider clientProvider,
            ICosmosContainerRegistry registry)
        {
            this.clientProvider = clientProvider;
            this.registry = registry;
        }

        public Container GetContainer<T>(
            bool allowBulk = false)
        {
            var container = registry.GetContainerForType<T>();
            var options = container.Options ?? registry.DefaultOptions;

            return GetClient(options, allowBulk)
                .GetContainer(
                    options.DatabaseName,
                    container.ContainerName);
        }

        public Container GetContainer(
            Type resourceType,
            bool allowBulk = false)
        {
            var container = registry.GetContainerForType(resourceType);
            var options = container.Options ?? registry.DefaultOptions;

            return GetClient(options, allowBulk)
                .GetContainer(
                    options.DatabaseName,
                    container.ContainerName);
        }

        public Container GetContainer(
            string name,
            bool allowBulk = false)
        {
            return GetClient(registry.DefaultOptions, allowBulk)
                .GetContainer(
                    registry.DefaultOptions.DatabaseName,
                    name);
        }

        public Container GetContainerWithName<T>(
            string name,
            bool allowBulk = false)
        {
            var container = registry.GetContainerForType<T>();
            var options = container.Options ?? registry.DefaultOptions;

            return GetClient(options, allowBulk)
                .GetContainer(
                    options.DatabaseName,
                    name);
        }

        public Container GetContainerWithName(
            Type resourceType,
            string name,
            bool allowBulk = false)
        {
            var container = registry.GetContainerForType(resourceType);
            var options = container.Options!;

            return GetClient(options, allowBulk)
                .GetContainer(
                    options.DatabaseName,
                    name);
        }

        public CosmosOptions GetCosmosOptions<T>()
            => registry
                .GetContainerForType<T>()
                .Options!;

        public CosmosOptions GetCosmosOptions(Type resourceType)
            => registry
                .GetContainerForType(resourceType)
                .Options!;

        private CosmosClient GetClient(CosmosOptions options, bool allowBulk)
            => allowBulk
             ? clientProvider.GetBulkClient(options)
             : clientProvider.GetClient(options);
    }
}