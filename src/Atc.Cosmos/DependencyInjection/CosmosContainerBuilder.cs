using System;
using Atc.Cosmos.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Atc.Cosmos.DependencyInjection
{
    public class CosmosContainerBuilder : ICosmosContainerBuilder
    {
        private readonly ICosmosContainerRegistry registry;

        public CosmosContainerBuilder(
            string containerName,
            IServiceCollection services,
            ICosmosContainerRegistry registry,
            string? databaseName)
        {
            this.ContainerName = containerName;
            this.Services = services;
            this.registry = registry;
            DatabaseName = databaseName;
        }

        public string ContainerName { get; }

        public string? DatabaseName { get; }

        public IServiceCollection Services { get; }

        public ICosmosContainerBuilder<TResource> AddResource<TResource>()
            where TResource : class, ICosmosResource
        {
            Services.AddSingleton(
                registry.Register<TResource>(ContainerName, DatabaseName));

            return new CosmosContainerBuilder<TResource>(
                ContainerName,
                Services,
                registry,
                DatabaseName);
        }

        public ICosmosContainerBuilder AddResource(
            Type resourceType)
        {
            Services.AddSingleton(
                registry.Register(resourceType, ContainerName, DatabaseName));

            return this;
        }
    }
}