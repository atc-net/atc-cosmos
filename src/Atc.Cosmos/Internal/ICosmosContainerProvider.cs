using Microsoft.Azure.Cosmos;

namespace Atc.Cosmos.Internal
{
    /// <summary>
    /// Factory is responsible for providing access to Cosmos DB containers.
    /// </summary>
    public interface ICosmosContainerProvider
    {
        Container GetContainer<T>();
    }
}