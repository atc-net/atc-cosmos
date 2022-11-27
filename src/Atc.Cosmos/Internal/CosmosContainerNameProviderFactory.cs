using System;
using System.Collections.Generic;
using System.Security.AccessControl;

namespace Atc.Cosmos.Internal
{
    public class CosmosContainerNameProviderFactory : ICosmosContainerNameProviderFactory
    {
        private readonly HashSet<Type> constraints = new HashSet<Type>();

        public ICosmosContainerNameProvider Register<T>(string containerName, CosmosOptions? options = default)
            where T : ICosmosResource
        {
            if (HasAlreadyBeenRegistered(typeof(T)))
            {
                throw new NotSupportedException(
                    $"Type {typeof(T).Name} can only be registered once.");
            }

            return new CosmosContainerNameProvider<T>(containerName, options);
        }

        public ICosmosContainerNameProvider Register(Type resourceType, string containerName, CosmosOptions? options = default)
        {
            if (HasAlreadyBeenRegistered(resourceType))
            {
                throw new NotSupportedException(
                    $"Type {resourceType.Name} can only be registered once.");
            }

            return new CosmosContainerNameProvider(resourceType, containerName, options);
        }

        private bool HasAlreadyBeenRegistered(Type type)
        {
            if (type.IsGenericTypeDefinition || !type.IsGenericType)
            {
                if (constraints.Contains(type))
                {
                    return true;
                }

                constraints.Add(type);
                return false;
            }

            if (type.IsGenericType && constraints.Contains(type.GetGenericTypeDefinition()))
            { // if the generic version has already been registered then go no further.
                return true;
            }

            if (constraints.Contains(type))
            {
                return true;
            }

            constraints.Add(type);
            return false;
        }
    }
}