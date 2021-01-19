using System;

namespace Atc.Cosmos.Internal
{
    public class CosmosContainerNameProvider<T> : ICosmosContainerNameProvider
        where T : ICosmosResource
    {
        public CosmosContainerNameProvider(
            string containerName)
        {
            ContainerName = containerName;
        }

        public Type FromType => typeof(T);
        public string ContainerName { get; }
    }
}