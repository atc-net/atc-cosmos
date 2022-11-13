using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace Atc.Cosmos.Internal
{
    public class CosmosContainerProvider : ICosmosContainerProvider
    {
        private readonly ICosmosClientProvider clientProvider;
        private readonly ICosmosDatabaseNameProvider databaseNameProvider;
        private readonly List<ICosmosContainerNameProvider> nameProviders;

        public CosmosContainerProvider(
            ICosmosClientProvider clientProvider,
            ICosmosDatabaseNameProvider databaseNameProvider,
            IEnumerable<ICosmosContainerNameProvider> nameProviders)
        {
            this.clientProvider = clientProvider;
            this.databaseNameProvider = databaseNameProvider;
            this.nameProviders = nameProviders.ToList();
        }

        public Container GetContainer<T>(
            bool allowBulk = false)
        {
            var container = GetContainerForType(typeof(T));
            return GetContainer(
                container.ContainerName,
                databaseNameProvider.ResolveDatabaseName(container),
                allowBulk);
        }

        public Container GetContainer(
            Type resourceType,
            bool allowBulk = false)
        {
            var container = GetContainerForType(resourceType);
            return GetContainer(
                container.ContainerName,
                databaseNameProvider.ResolveDatabaseName(container),
                allowBulk);
        }

        public Container GetContainer(
            string name,
            string databaseName,
            bool allowBulk = false)
            => GetClient(allowBulk)
                .GetContainer(
                    databaseName,
                    name);

        public Container GetContainer(
            string name,
            bool allowBulk = false)
            => GetClient(allowBulk)
                .GetContainer(
                    databaseNameProvider.DefaultDatabaseName,
                    name);

        public string GetDatabaseFor<T>()
            => databaseNameProvider.ResolveDatabaseName(GetContainerForType(typeof(T)));

        private ICosmosContainerNameProvider GetContainerForType(
            Type resourceType)
            => nameProviders.FirstOrDefault(p => p.IsForType(resourceType))
            ?? throw new NotSupportedException(
                $"Type {resourceType.Name} is not supported.");

        private CosmosClient GetClient(bool allowBulk)
            => allowBulk
             ? clientProvider.GetBulkClient()
             : clientProvider.GetClient();
    }
}