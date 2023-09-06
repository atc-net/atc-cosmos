using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace Atc.Cosmos.Internal
{
    public class CosmosBulkWriter<T> : ICosmosBulkWriter<T>
        where T : class, ICosmosResource
    {
        private readonly Container container;

        public CosmosBulkWriter(
            ICosmosContainerProvider containerProvider)
        {
            this.container = containerProvider.GetContainer<T>(allowBulk: true);
        }

#if PREVIEW
        protected virtual PriorityLevel PriorityLevel => PriorityLevel.High;
#endif

        public Task CreateAsync(
            T document,
            CancellationToken cancellationToken = default)
            => container
                .CreateItemAsync<object>(
                    document,
                    new PartitionKey(document.PartitionKey),
                    new ItemRequestOptions
                    {
                        EnableContentResponseOnWrite = false,
#if PREVIEW
                        PriorityLevel = PriorityLevel,
#endif
                    },
                    cancellationToken);

        public Task WriteAsync(
            T document,
            CancellationToken cancellationToken = default)
            => container
                .UpsertItemAsync<object>(
                    document,
                    new PartitionKey(document.PartitionKey),
                    new ItemRequestOptions
                    {
                        EnableContentResponseOnWrite = false,
#if PREVIEW
                        PriorityLevel = PriorityLevel,
#endif
                    },
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
#if PREVIEW
                        PriorityLevel = PriorityLevel,
#endif
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
                    new ItemRequestOptions
                    {
                        EnableContentResponseOnWrite = false,
#if PREVIEW
                        PriorityLevel = PriorityLevel,
#endif
                    },
                    cancellationToken: cancellationToken);
    }
}