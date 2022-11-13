using System;

namespace Atc.Cosmos.Internal
{
    public interface ICosmosContainerRegistry
    {
        ICosmosContainerNameProvider Register<T>(string containerName, string? databaseName)
            where T : ICosmosResource;

        ICosmosContainerNameProvider Register(Type resourceType, string containerName, string? databaseName);
    }
}