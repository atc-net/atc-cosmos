using Atc.Cosmos.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Atc.Cosmos.DependencyInjection
{
    public class CosmosContainerBuilder : ICosmosContainerBuilder
    {
        public CosmosContainerBuilder(
            string containerName,
            IServiceCollection services)
        {
            this.ContainerName = containerName;
            this.Services = services;
        }

        public string ContainerName { get; }

        public IServiceCollection Services { get; }

        public ICosmosContainerBuilder<TResource> AddResource<TResource>()
            where TResource : class, ICosmosResource
        {
            Services.AddSingleton<ICosmosContainerNameProvider>(
                new CosmosContainerNameProvider<TResource>(ContainerName));

            return new CosmosContainerBuilder<TResource>(
                ContainerName,
                Services);
        }
    }
}