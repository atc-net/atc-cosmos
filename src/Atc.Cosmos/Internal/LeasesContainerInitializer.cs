using System.Threading;
using System.Threading.Tasks;
using Atc.Cosmos;
using Microsoft.Azure.Cosmos;

namespace Atc.Cosmos.Internal
{
    public class LeasesContainerInitializer : ICosmosContainerInitializer
    {
        public const string ContainerId = "leases";

        public Task InitializeAsync(
            Database database,
            CancellationToken cancellationToken)
        {
            var options = new ContainerProperties
            {
                IndexingPolicy = new IndexingPolicy
                {
                    Automatic = true,
                    IndexingMode = IndexingMode.Consistent,
                },
                Id = ContainerId,
                PartitionKeyPath = "/id",
            };

            options.IndexingPolicy.ExcludedPaths.Add(new ExcludedPath { Path = "/*" });

            return database.CreateContainerIfNotExistsAsync(
                options,
                cancellationToken: cancellationToken);
        }
    }
}