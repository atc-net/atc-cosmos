using Atc.Cosmos.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Atc.Cosmos.DependencyInjection
{
    public class CosmosBuilder<T> : CosmosBuilder, ICosmosBuilder<T>
        where T : class, ICosmosResource
    {
        public CosmosBuilder(
            IServiceCollection services,
            ICosmosContainerNameProviderFactory containerRegistry,
            CosmosOptions? options)
            : base(services, containerRegistry, options)
        {
        }

        public ICosmosBuilder<T> WithChangeFeedProcessor<TProcessor>(
            int maxDegreeOfParallelism = 1)
            where TProcessor : class, IChangeFeedProcessor<T>
        {
            var changeFeedProcessorOptions = new ChangeFeedProcessorOptions()
            {
                MaxDegreeOfParallelism = maxDegreeOfParallelism,
            };
            return WithChangeFeedProcessor<TProcessor>(changeFeedProcessorOptions);
        }

        public ICosmosBuilder<T> WithChangeFeedProcessor<TProcessor>(
            ChangeFeedProcessorOptions changeFeedProcessorOptions)
            where TProcessor : class, IChangeFeedProcessor<T>
        {
            Services.AddSingleton<LeasesContainerInitializer>();
            Services.AddSingleton<IScopedCosmosContainerInitializer>(
                s => new ScopedCosmosContainerInitializer(
                    Options,
                    s.GetRequiredService<LeasesContainerInitializer>()));

            Services.TryAddSingleton<IChangeFeedFactory, ChangeFeedFactory>();
            Services.AddSingleton<TProcessor>();

            Services.AddSingleton(s => new ChangeFeedListener<T, TProcessor>(
                s.GetRequiredService<IChangeFeedFactory>(),
                s.GetRequiredService<TProcessor>(),
                changeFeedProcessorOptions.MaxDegreeOfParallelism));

            Services.AddSingleton<IChangeFeedListener, ChangeFeedListener<T, TProcessor>>(
                s => s.GetRequiredService<ChangeFeedListener<T, TProcessor>>());
            Services.AddSingleton<IChangeFeedListener<T>, ChangeFeedListener<T, TProcessor>>(
                s => s.GetRequiredService<ChangeFeedListener<T, TProcessor>>());

            return this;
        }
    }
}