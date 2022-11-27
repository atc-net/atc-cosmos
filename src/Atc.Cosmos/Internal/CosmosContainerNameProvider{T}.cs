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
            CosmosOptions? options)
        {
            ContainerName = containerName;
            Options = options;
        }

        public string ContainerName { get; }

        public CosmosOptions? Options { get; set; }

        public bool IsForType(Type resourceType) => typeof(T) == resourceType;
    }
}