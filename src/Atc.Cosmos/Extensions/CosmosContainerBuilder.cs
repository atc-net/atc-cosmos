using Atc.Cosmos.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Atc.Cosmos.Extensions
{
    public class CosmosContainerBuilder : ICosmosContainerBuilder
    {
        public string ContainerName { get; }
        public IServiceCollection Services { get; }

        public CosmosContainerBuilder(
            string containerName,
            IServiceCollection services)
        {
            this.ContainerName = containerName;
            this.Services = services;
        }

        public ICosmosContainerBuilder AddResource<TResource>()
            where TResource : ICosmosResource
        {
            Services.AddSingleton<ICosmosContainerNameProvider>(
                new CosmosContainerNameProvider<TResource>(ContainerName));

            return this;
        }
    }
}