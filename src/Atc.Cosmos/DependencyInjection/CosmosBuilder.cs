using System;
using Atc.Cosmos.Internal;
using Atc.Cosmos.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Atc.Cosmos.DependencyInjection
{
    public class CosmosBuilder : ICosmosBuilder
    {
        private readonly ICosmosContainerNameProviderFactory containerRegistry;

        public CosmosBuilder(
            IServiceCollection services,
            ICosmosContainerNameProviderFactory containerRegistry,
            CosmosOptions? options)
        {
            this.Services = services;
            this.containerRegistry = containerRegistry;
            this.Options = options;
        }

        public IServiceCollection Services { get; }

        public CosmosOptions? Options { get; }

        public ICosmosBuilder AddContainer(
            string name,
            Action<ICosmosContainerBuilder> builder)
        {
            builder(new CosmosContainerBuilder(name, Services, containerRegistry, Options));
            return this;
        }

        public ICosmosBuilder AddContainer<TInitializer>(
            string name,
            Action<ICosmosContainerBuilder> builder)
            where TInitializer : class, ICosmosContainerInitializer
        {
            Services.AddSingleton<ICosmosContainerInitializer, TInitializer>();

            return AddContainer(name, builder);
        }

        public ICosmosBuilder<TResource> AddContainer<TInitializer, TResource>(
           string name)
           where TInitializer : class, ICosmosContainerInitializer
           where TResource : class, ICosmosResource
        {
            Services.AddSingleton<ICosmosContainerInitializer, TInitializer>();

            return AddContainer<TResource>(name);
        }

        public ICosmosBuilder AddContainer<TInitializer>(
            Type resourceType,
            string name)
            where TInitializer : class, ICosmosContainerInitializer
        {
            Services.AddSingleton<ICosmosContainerInitializer, TInitializer>();

            return AddContainer(resourceType, name);
        }

        public ICosmosBuilder<TResource> AddContainer<TResource>(
            string name)
            where TResource : class, ICosmosResource
        {
            Services.AddSingleton(
                containerRegistry.Register<TResource>(name, Options));

            return new CosmosBuilder<TResource>(Services, containerRegistry, Options);
        }

        public ICosmosBuilder AddContainer(
            Type resourceType,
            string name)
        {
            Services.AddSingleton(
                containerRegistry.Register(resourceType, name, Options));

            return this;
        }

        public ICosmosBuilder UseHostedService()
        {
            Services.AddHostedService<StartupInitializationJob>();
            Services.AddHostedService<ChangeFeedService>();

            return this;
        }

        public ICosmosBuilder ForDatabase(CosmosOptions options)
        {
            return new CosmosBuilder(Services, containerRegistry, options);
        }
    }
}