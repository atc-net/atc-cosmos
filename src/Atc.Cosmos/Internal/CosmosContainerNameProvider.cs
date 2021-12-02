using System;

namespace Atc.Cosmos.Internal
{
    public class CosmosContainerNameProvider : ICosmosContainerNameProvider
    {
        private readonly Type containerType;
        private readonly string containerName;

        public CosmosContainerNameProvider(
            Type resourceType,
            string containerName)
        {
            this.containerType = resourceType;
            this.containerName = containerName;
        }

        public string? GetContainerName(Type resourceType)
        {
            if (containerType.IsGenericTypeDefinition)
            {
                if (resourceType.GetGenericTypeDefinition() == containerType)
                {
                    return containerName;
                }
            }
            else if (containerType == resourceType)
            {
                return containerName;
            }

            return null;
        }
    }
}