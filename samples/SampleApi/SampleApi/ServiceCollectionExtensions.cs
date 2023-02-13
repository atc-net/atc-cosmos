namespace SampleApi;

public static class ServiceCollectionExtensions
{
    public static void ConfigureCosmosDb(this IServiceCollection services)
    {
        services.ConfigureOptions<ConfigureCosmosOptions>();
        services.ConfigureCosmos(
            cosmosBuilder =>
            {
                cosmosBuilder.AddContainer<FooContainerInitializer, FooResource>("foo");
                cosmosBuilder.UseHostedService();
            });
    }
}