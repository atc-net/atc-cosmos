using System;
using Atc.Cosmos.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Atc.Cosmos.DependencyInjection
{
    public class CosmosBuilder : ICosmosBuilder
    {
        private readonly ICosmosContainerRegistry containerRegistry;

        public CosmosBuilder(
            IServiceCollection services,
            ICosmosContainerRegistry containerRegistry,
            string? databaseName)
        {
            this.Services = services;
            this.containerRegistry = containerRegistry;
            DatabaseName = databaseName;
        }

        public IServiceCollection Services { get; }

        public string? DatabaseName { get; }

        public ICosmosBuilder AddContainer(
            string name,
            Action<ICosmosContainerBuilder> builder)
        {
            builder(new CosmosContainerBuilder(name, Services, containerRegistry, DatabaseName));
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
                containerRegistry.Register<TResource>(name, DatabaseName));

            return new CosmosBuilder<TResource>(Services, containerRegistry, DatabaseName);
        }

        public ICosmosBuilder AddContainer(
            Type resourceType,
            string name)
        {
            Services.AddSingleton(
                containerRegistry.Register(resourceType, name, DatabaseName));

            return this;
        }

        public ICosmosBuilder UseHostedService()
        {
            Services.AddHostedService<StartupInitializationJob>();
            Services.AddHostedService<ChangeFeedService>();

            return this;
        }

        public ICosmosBuilder ForDatabase(string databaseName)
            => new CosmosBuilder(Services, containerRegistry, databaseName);
    }
}