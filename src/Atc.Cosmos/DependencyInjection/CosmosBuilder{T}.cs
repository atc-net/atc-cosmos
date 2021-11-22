using Atc.Cosmos.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Atc.Cosmos.DependencyInjection
{
    public class CosmosBuilder<T> : CosmosBuilder, ICosmosBuilder<T>
        where T : class, ICosmosResource
    {
        public CosmosBuilder(
            IServiceCollection services)
            : base(services)
        {
        }

        public ICosmosBuilder<T> WithChangeFeedProcessor<TProcessor>()
            where TProcessor : class, IChangeFeedProcessor<T>
        {
            Services.AddSingleton<ICosmosContainerInitializer, LeasesContainerInitializer>();
            Services.TryAddSingleton<IChangeFeedFactory, ChangeFeedFactory>();
            Services.AddSingleton<TProcessor>();
            Services.AddSingleton<IChangeFeedListener, ChangeFeedListener<T, TProcessor>>();

            return this;
        }
    }
}