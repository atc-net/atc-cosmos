using System;
using System.Threading.Tasks;
using Atc.Cosmos.Serialization;
using Microsoft.Azure.Cosmos;

namespace Atc.Cosmos.Internal
{
    public static class ItemResponseExtensions
    {
        public static T GetResourceWithEtag<T>(
            this ItemResponse<T> response)
            where T : ICosmosResource
        {
            var resource = response.Resource;
            resource.ETag = response.ETag;

            return resource;
        }

        public static async Task<T> GetResourceWithEtag<T>(
            this Task<ItemResponse<T>> responseTask)
            where T : ICosmosResource
            => GetResourceWithEtag(
                await responseTask.ConfigureAwait(false));

        public static T GetResourceWithEtag<T>(
            this ItemResponse<object> response,
            IJsonCosmosSerializer serializer)
            where T : ICosmosResource
        {
            var json = response.Resource?.ToString();
            if (json is null)
            {
                throw new ArgumentException(
                    "ItemResponse did not provide a valid resource",
                    nameof(response));
            }

            var resource = serializer.FromString<T>(json);
            if (resource is null)
            {
                throw new ArgumentException(
                    $"ItemResponse did not provide valid json '{json}'",
                    nameof(response));
            }

            resource.ETag = response.ETag;

            return resource;
        }

        public static async Task<T> GetResourceWithEtag<T>(
            this Task<ItemResponse<object>> responseTask,
            IJsonCosmosSerializer serializer)
            where T : ICosmosResource
            => GetResourceWithEtag<T>(
                await responseTask.ConfigureAwait(false),
                serializer);
    }
}