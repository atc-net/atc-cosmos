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
        /// <param name="builder">The builder instance.</param>
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