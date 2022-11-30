using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace Atc.Cosmos.Internal
{
    public class CosmosContainerRegistry : ICosmosContainerRegistry
    {
        private readonly IOptions<CosmosOptions> defaultOptions;
        private readonly List<ICosmosContainerNameProvider> providers;

        public CosmosContainerRegistry(
            IOptions<CosmosOptions> defaultOptions,
            IEnumerable<ICosmosContainerNameProvider> nameProviders)
        {
            this.defaultOptions = defaultOptions;
            this.providers = nameProviders
                .Select(PatchDefaultOptions)
                .ToList();

            Options = providers
                .Select(p => p.Options!)
                .Union(new[] { defaultOptions.Value })
                .Distinct()
                .ToList();

            if (Options.Any(options => !IsValid(options)))
            {
                throw new InvalidOperationException(
                    $"Invalid configuration in {nameof(CosmosOptions)}.");
            }
        }

        public CosmosOptions DefaultOptions => defaultOptions.Value;

        public IReadOnlyList<CosmosOptions> Options { get; }

        public ICosmosContainerNameProvider GetContainerForType<TType>()
            => GetContainerForType(typeof(TType));

        public ICosmosContainerNameProvider GetContainerForType(Type resourceType)
            => providers.FirstOrDefault(p => p.IsForType(resourceType))
            ?? throw new NotSupportedException(
                $"Type {resourceType.Name} is not supported.");

        private static bool IsValid(CosmosOptions options)
            => options is not null
            && !string.IsNullOrEmpty(options.AccountEndpoint)
            && (!string.IsNullOrEmpty(options.AccountKey) || options.Credential is not null)
            && !string.IsNullOrEmpty(options.DatabaseName);

        private ICosmosContainerNameProvider PatchDefaultOptions(ICosmosContainerNameProvider provider)
        {
            if (provider.Options == null)
            {
                provider.Options = defaultOptions.Value;
            }

            return provider;
        }
    }
}