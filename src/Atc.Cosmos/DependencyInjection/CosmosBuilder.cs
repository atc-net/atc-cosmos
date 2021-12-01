using System;
using Atc.Cosmos.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Atc.Cosmos.DependencyInjection
{
    public class CosmosBuilder : ICosmosBuilder
    {
        public CosmosBuilder(
            IServiceCollection services)
        {
            this.Services = services;
        }

        public IServiceCollection Services { get; }

        public ICosmosBuilder AddContainer(
            string name,
            Action<ICosmosContainerBuilder> builder)
        {
            builder(new CosmosContainerBuilder(name, Services));
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
            Services.AddSingleton<ICosmosContainerNameProvider>(
                new CosmosContainerNameProvider<TResource>(name));

            return new CosmosBuilder<TResource>(Services);
        }

        public ICosmosBuilder AddContainer(
            Type resourceType,
            string name)
        {
            Services.AddSingleton<ICosmosContainerNameProvider>(
                new CosmosContainerNameProvider(resourceType, name));

            return this;
        }

        public ICosmosBuilder UseHostedService()
        {
            Services.AddHostedService<StartupInitializationJob>();
            Services.AddHostedService<ChangeFeedService>();

            return this;
        }
    }
}