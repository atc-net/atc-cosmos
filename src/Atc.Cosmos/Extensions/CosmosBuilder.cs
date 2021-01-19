using System;
using Atc.Cosmos.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Atc.Cosmos.Extensions
{
    public class CosmosBuilder : ICosmosBuilder
    {
        public IServiceCollection Services { get; }

        public CosmosBuilder(
            IServiceCollection services)
        {
            this.Services = services;
        }

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

        public ICosmosBuilder AddContainer<TInitializer, TResource>(
           string name)
           where TInitializer : class, ICosmosContainerInitializer
           where TResource : class, ICosmosResource
        {
            Services.AddSingleton<ICosmosContainerInitializer, TInitializer>();

            return AddContainer<TResource>(name);
        }

        public ICosmosBuilder AddContainer<TResource>(
            string name)
            where TResource : class, ICosmosResource
        {
            Services.AddSingleton<ICosmosContainerNameProvider>(
                new CosmosContainerNameProvider<TResource>(name));

            return this;
        }

        public ICosmosBuilder UseHostedService()
        {
            Services.AddHostedService<StartupInitializationJob>();

            return this;
        }
    }
}