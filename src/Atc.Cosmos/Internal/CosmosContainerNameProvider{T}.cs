using System;
using System.Collections.Generic;
using System.Security.AccessControl;

namespace Atc.Cosmos.Internal
{
    public class CosmosContainerNameProvider<T> : ICosmosContainerNameProvider
        where T : ICosmosResource
    {
        public CosmosContainerNameProvider(
            string containerName,
            string? databaseName)
        {
            ContainerName = containerName;
            DatabaseName = databaseName;
        }

        public string ContainerName { get; }

        public string? DatabaseName { get; }

        public bool IsForType(Type resourceType) => typeof(T) == resourceType;
    }
}