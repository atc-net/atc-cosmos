using System;
using System.Collections.Generic;
using System.Linq;

namespace Atc.Cosmos.Internal
{
    public class CosmosContainerNameProvider : ICosmosContainerNameProvider
    {
        private readonly Type containerType;

        public CosmosContainerNameProvider(
            Type resourceType,
            string containerName,
            string? databaseName)
        {
            this.containerType = resourceType;
            this.ContainerName = containerName;
            this.DatabaseName = databaseName;
        }

        public string? DatabaseName { get; }

        public string ContainerName { get; }

        public bool IsForType(Type resourceType)
        {
            if (containerType.IsGenericTypeDefinition)
            {
                if (resourceType.GetGenericTypeDefinition() == containerType)
                {
                    return true;
                }
            }
            else if (containerType == resourceType)
            {
                return true;
            }

            return false;
        }
    }
}