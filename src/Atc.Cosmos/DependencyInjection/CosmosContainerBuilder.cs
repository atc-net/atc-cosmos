using System;
using Atc.Cosmos.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Atc.Cosmos.DependencyInjection
{
    public class CosmosContainerBuilder : ICosmosContainerBuilder
    {
        private readonly ICosmosContainerNameProviderFactory registry;

        public CosmosContainerBuilder(
            string containerName,
            IServiceCollection services,
            ICosmosContainerNameProviderFactory registry,
            CosmosOptions? options)
        {
            this.ContainerName = containerName;
            this.Services = services;
            this.registry = registry;
            this.Options = options;
        }

        public string ContainerName { get; }

        public CosmosOptions? Options { get; }

        public IServiceCollection Services { get; }

        public ICosmosContainerBuilder<TResource> AddResource<TResource>()
            where TResource : class, ICosmosResource
        {
            Services.AddSingleton(
                registry.Register<TResource>(ContainerName, Options));

            return new CosmosContainerBuilder<TResource>(
                ContainerName,
                Services,
                registry,
                Options);
        }

        public ICosmosContainerBuilder AddResource(
            Type resourceType)
        {
            Services.AddSingleton(
                registry.Register(resourceType, ContainerName, Options));

            return this;
        }
    }
}