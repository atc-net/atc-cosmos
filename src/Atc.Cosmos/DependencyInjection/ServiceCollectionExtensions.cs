using System;
using System.Threading;
using Atc.Cosmos;
using Atc.Cosmos.DependencyInjection;
using Atc.Cosmos.Internal;
using Atc.Cosmos.Serialization;
using Microsoft.Extensions.Hosting;
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
            services.AddSingleton(typeof(ICosmosBulkWriter<>), typeof(CosmosBulkWriter<>));
            services.AddSingleton<ICosmosInitializer, CosmosInitializer>();
            services.AddSingleton<IJsonCosmosSerializer, JsonCosmosSerializer>();
            services.AddSingleton<ICosmosClientProvider, CosmosClientProvider>();

            builder(new CosmosBuilder(services));
            return services;
        }

        /// <summary>
        /// Executes the Cosmos initialization logic.
        /// </summary>
        /// <remarks>
        /// As Azure Functions does not have a mechanism for executing
        /// asynchronous initialization logic, this method should be called
        /// before running the <c>IHost</c>.
        /// </remarks>
        /// <param name="services">The service provider.</param>
        /// <returns>The same service provider.</returns>
        public static IServiceProvider AzureFunctionInitializeCosmosDatabase(
            this IServiceProvider services)
        {
            services
                .GetRequiredService<ICosmosInitializer>()
                .InitializeAsync(CancellationToken.None)
                .GetAwaiter().GetResult();

            return services;
        }

        /// <summary>
        /// Executes the Cosmos initialization logic.
        /// </summary>
        /// <remarks>
        /// As Azure Functions does not have a mechanism for executing
        /// asynchronous initialization logic, this method should be called
        /// before running the <c>IHost</c>.
        /// </remarks>
        /// <param name="host">The host.</param>
        /// <returns>The same host.</returns>
        public static IHost AzureFunctionInitializeCosmosDatabase(
            this IHost host)
        {
            host
                .Services
                .GetRequiredService<ICosmosInitializer>()
                .InitializeAsync(CancellationToken.None)
                .GetAwaiter().GetResult();

            return host;
        }

        /// <summary>
        /// Executes the Cosmos initialization logic.
        /// </summary>
        /// <remarks>
        /// As Azure Functions does not have a mechanism for executing
        /// asynchronous initialization logic, this method should be called
        /// before running the <c>IHost</c>.
        /// </remarks>
        /// <param name="services">The service collection.</param>
        public static void AzureFunctionInitializeCosmosDatabase(
            this IServiceCollection services)
            => services
                .BuildServiceProvider()
                .AzureFunctionInitializeCosmosDatabase();
    }
}