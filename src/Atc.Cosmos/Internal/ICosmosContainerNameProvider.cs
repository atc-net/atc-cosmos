using System;

namespace Atc.Cosmos.Internal
{
    public interface ICosmosContainerNameProvider
    {
        Type FromType { get; }

        string ContainerName { get; }
    }
}