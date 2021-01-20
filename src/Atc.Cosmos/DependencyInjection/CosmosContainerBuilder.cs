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

        public ICosmosContainerBuilder AddResource<TResource>()
            where TResource : ICosmosResource
        {
            Services.AddSingleton<ICosmosContainerNameProvider>(
                new CosmosContainerNameProvider<TResource>(ContainerName));

            return this;
        }
    }
}