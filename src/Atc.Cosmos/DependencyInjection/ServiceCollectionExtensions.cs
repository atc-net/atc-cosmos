using System;
using System.Threading;
using Atc.Cosmos;
using Atc.Cosmos.DependencyInjection;
using Atc.Cosmos.Internal;
using Atc.Cosmos.Serialization;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Configures Cosmos by allowing the caller to access the
        /// <see cref="ICosmosBuilder"/>.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="options">The <see cref="CosmosOptions"/> to be used.</param>
        /// <param name="builder">The builder method, for configuring Cosmos.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection ConfigureCosmos(
           this IServiceCollection services,
           CosmosOptions options,
           Action<ICosmosBuilder> builder)
            => services
                .ConfigureCosmos(s => options, builder);

        /// <summary>
        /// Configures Cosmos by allowing the caller to access the
        /// <see cref="ICosmosBuilder"/>.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="optionsFactory">A factory method used to create the <see cref="CosmosOptions"/> instance.</param>
        /// <param name="builder">The builder method, for configuring Cosmos.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection ConfigureCosmos(
           this IServiceCollection services,
           Func<IServiceProvider, CosmosOptions> optionsFactory,
           Action<ICosmosBuilder> builder)
            => services
                .AddSingleton<IOptions<CosmosOptions>>(s =>
                    new OptionsWrapper<CosmosOptions>(
                        optionsFactory(s)))
                .ConfigureCosmos(builder);

        /// <summary>
        /// Configures Cosmos by allowing the caller to access the
        /// <see cref="ICosmosBuilder"/>.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="builder">The builder method, for configuring Cosmos.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection ConfigureCosmos(
           this IServiceCollection services,
           Action<ICosmosBuilder> builder)
        {
            services.AddSingleton<ICosmosContainerProvider, CosmosContainerProvider>();
            services.AddSingleton(typeof(ICosmosReader<>), typeof(CosmosReader<>));
            services.AddSingleton(typeof(ICosmosWriter<>), typeof(CosmosWriter<>));
            services.AddSingleton<ICosmosInitializer, CosmosInitializer>();
            services.AddSingleton<IJsonCosmosSerializer, JsonCosmosSerializer>();

            services.AddSingleton(s =>
            {
                var options = s.GetRequiredService<IOptions<CosmosOptions>>().Value;
                if (!options.IsValid())
                {
                    throw new InvalidOperationException(
                        $"Invalid configuration in {nameof(CosmosOptions)}.");
                }

                var connectionString = $"AccountEndpoint={options.AccountEndpoint};AccountKey={options.AccountKey}";

                var clientOptions = s.GetService<IOptions<CosmosClientOptions>>()?.Value
                    ?? new CosmosClientOptions();

                clientOptions.Serializer = new CosmosSerializerAdapter(
                    s.GetRequiredService<IJsonCosmosSerializer>());

                return new CosmosClient(connectionString, clientOptions);
            });

            builder(new CosmosBuilder(services));
            return services;
        }

        /// <summary>
        /// Executes the Cosmos initialization logic.
        /// </summary>
        /// <remarks>
        /// As Azure Functions does not have a mechanism for executing
        /// asynchronous initialization logic, this method should be called
        /// during setup of the <c>IWebJobsBuilder</c>.
        /// </remarks>
        /// <param name="services">The service provider.</param>
        public static void AzureFunctionInitializeCosmosDatabase(
            this IServiceProvider services)
            => services
                .GetRequiredService<ICosmosInitializer>()
                .InitializeAsync(CancellationToken.None)
                .GetAwaiter().GetResult();

        /// <summary>
        /// Executes the Cosmos initialization logic.
        /// </summary>
        /// <remarks>
        /// As Azure Functions does not have a mechanism for executing
        /// asynchronous initialization logic, this method should be called
        /// during setup of the <c>IWebJobsBuilder</c>.
        /// </remarks>
        /// <param name="services">The service collection.</param>
        public static void AzureFunctionInitializeCosmosDatabase(
            this IServiceCollection services)
            => services
                .BuildServiceProvider()
                .AzureFunctionInitializeCosmosDatabase();

        private static bool IsValid(this CosmosOptions options)
            => !string.IsNullOrEmpty(options.AccountEndpoint)
            && !string.IsNullOrEmpty(options.AccountKey)
            && !string.IsNullOrEmpty(options.DatabaseName);
    }
}