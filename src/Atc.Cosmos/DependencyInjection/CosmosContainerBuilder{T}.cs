using Atc.Cosmos.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Atc.Cosmos.DependencyInjection
{
    public class CosmosContainerBuilder<T> : CosmosContainerBuilder, ICosmosContainerBuilder<T>
        where T : class, ICosmosResource
    {
        public CosmosContainerBuilder(
            string containerName,
            IServiceCollection services)
            : base(containerName, services)
        {
        }

        public ICosmosContainerBuilder<T> WithChangeFeedProcessor<TProcessor>(
            int maxDegreeOfParallelism = 1)
            where TProcessor : class, IChangeFeedProcessor<T>
        {
            Services.AddSingleton<ICosmosContainerInitializer, LeasesContainerInitializer>();
            Services.TryAddSingleton<IChangeFeedFactory, ChangeFeedFactory>();
            Services.AddSingleton<TProcessor>();

            Services.AddSingleton(s => new ChangeFeedListener<T, TProcessor>(
                s.GetRequiredService<IChangeFeedFactory>(),
                s.GetRequiredService<TProcessor>(),
                maxDegreeOfParallelism));

            Services.AddSingleton<IChangeFeedListener, ChangeFeedListener<T, TProcessor>>(
                s => s.GetRequiredService<ChangeFeedListener<T, TProcessor>>());
            Services.AddSingleton<IChangeFeedListener<T>, ChangeFeedListener<T, TProcessor>>(
                s => s.GetRequiredService<ChangeFeedListener<T, TProcessor>>());

            return this;
        }
    }
}