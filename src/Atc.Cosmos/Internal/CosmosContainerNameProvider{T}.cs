using System;

namespace Atc.Cosmos.Internal
{
    public class CosmosContainerNameProvider<T> : ICosmosContainerNameProvider
        where T : ICosmosResource
    {
        private readonly string containerName;

        public CosmosContainerNameProvider(
            string containerName)
        {
            this.containerName = containerName;
        }

        public string? GetContainerName(Type resourceType)
            => typeof(T) == resourceType
             ? containerName
             : null;
    }
}