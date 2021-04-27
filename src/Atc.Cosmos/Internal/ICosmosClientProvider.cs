using Microsoft.Azure.Cosmos;

namespace Atc.Cosmos.Internal
{
    public interface ICosmosClientProvider
    {
        CosmosClient GetClient();

        CosmosClient GetBulkClient();
    }
}