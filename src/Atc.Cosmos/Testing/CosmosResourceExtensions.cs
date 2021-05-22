using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Atc.Cosmos.Testing
{
    public static class CosmosResourceExtensions
    {
        public static TResource Clone<TResource>(
            this TResource resource,
            JsonSerializerOptions? options = null)
        {
            if (resource is null)
            {
                return resource;
            }

            var json = JsonSerializer.Serialize(resource, options);
            return JsonSerializer.Deserialize<TResource>(json, options)
                ?? resource;
        }

        public static IEnumerable<TResource> Clone<TResource>(
            this IEnumerable<TResource> resources,
            JsonSerializerOptions? options = null)
            => resources.Select(r => r.Clone(options));
    }
}
