using System.Threading;
using System.Threading.Tasks;
using Atc.Cosmos.Serialization;
using Microsoft.Azure.Cosmos;

namespace Atc.Cosmos.Internal
{
    public class CosmosBulkWriter<T> : ICosmosBulkWriter<T>
        where T : class, ICosmosResource
    {
        private readonly Container container;
        private readonly IJsonCosmosSerializer serializer;

        public CosmosBulkWriter(
            ICosmosContainerProvider containerProvider,
            IJsonCosmosSerializer serializer)
        {
            this.container = containerProvider.GetContainer<T>(allowBulk: true);
            this.serializer = serializer;
        }

        public Task CreateAsync(
            T document,
            CancellationToken cancellationToken = default)
            => container
                .CreateItemAsync<object>(
                    document,
                    new PartitionKey(document.PartitionKey),
                    new ItemRequestOptions { EnableContentResponseOnWrite = false },
                    cancellationToken);

        public Task WriteAsync(
            T document,
            CancellationToken cancellationToken = default)
            => container
                .UpsertItemAsync<object>(
                    document,
                    new PartitionKey(document.PartitionKey),
                    new ItemRequestOptions { EnableContentResponseOnWrite = false },
                    cancellationToken);

        public Task ReplaceAsync(
            T document,
            CancellationToken cancellationToken = default)
            => container
                .ReplaceItemAsync<object>(
                    document,
                    document.DocumentId,
                    new PartitionKey(document.PartitionKey),
                    new ItemRequestOptions
                    {
                        IfMatchEtag = document.ETag,
                        EnableContentResponseOnWrite = false,
                    },
                    cancellationToken);

        public Task DeleteAsync(
            string documentId,
            string partitionKey,
            CancellationToken cancellationToken = default)
            => container
                .DeleteItemAsync<object>(
                    documentId,
                    new PartitionKey(partitionKey),
                    new ItemRequestOptions { EnableContentResponseOnWrite = false },
                    cancellationToken: cancellationToken);
    }
}