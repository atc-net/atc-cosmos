using Atc.Cosmos;
using Atc.Cosmos.AutoIncrement;
using Atc.Cosmos.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class CosmosBuilderExtensions
    {
        /// <summary>
        /// Adds the <see cref="IAutoIncrementProvider"/> and dependencies.
        /// </summary>
        /// <param name="builder">The <see cref="ICosmosBuilder"/> instance.</param>
        /// <returns>The <see cref="ICosmosBuilder"/> so that additional calls can be chained.</returns>
        public static ICosmosBuilder AddAutoIncrementProvider(
            this ICosmosBuilder builder)
        {
            builder.Services.AddSingleton<IAutoIncrementProvider, AutoIncrementProvider>();
            builder.AddContainer<AutoIncrementCounterInitializer, AutoIncrementCounter>(
                AutoIncrementCounterInitializer.ContainerId);

            return builder;
        }
    }
}