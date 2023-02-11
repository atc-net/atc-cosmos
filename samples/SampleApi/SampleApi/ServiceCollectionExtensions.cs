using Atc.Cosmos;

namespace SampleApi;

public static class ServiceCollectionExtensions
{
    public static void ConfigureCosmosDb(this IServiceCollection services)
    {
        // Configure Atc.Cosmos
        services.AddOptions<CosmosOptions>();
        services.ConfigureOptions<ConfigureCosmosOptions>();
        services.ConfigureCosmos(
            cosmosBuilder =>
            {
                cosmosBuilder.AddContainer<FooContainerInitializer, FooResource>("foo");
                cosmosBuilder.UseHostedService();
            });
    }
}