using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Atc.Cosmos.Serialization;
using Microsoft.Azure.Cosmos;

namespace Atc.Cosmos.Internal
{
    public class CosmosWriter<T> : ICosmosWriter<T>
        where T : class, ICosmosResource
    {
        private readonly Container container;
        private readonly ICosmosReader<T> reader;
        private readonly IJsonCosmosSerializer serializer;

        public CosmosWriter(
            ICosmosContainerProvider containerProvider,
            ICosmosReader<T> reader,
            IJsonCosmosSerializer serializer)
        {
            this.container = containerProvider.GetContainer<T>();
            this.reader = reader;
            this.serializer = serializer;
        }

        public Task<T> CreateAsync(
            T document,
            CancellationToken cancellationToken = default)
            => container
                .CreateItemAsync<object>(
                    document,
                    new PartitionKey(document.PartitionKey),
                    new ItemRequestOptions { },
                    cancellationToken)
                .GetResourceWithEtag<T>(serializer);

        public Task<T> WriteAsync(
            T document,
            CancellationToken cancellationToken = default)
            => container
                .UpsertItemAsync<object>(
                    document,
                    new PartitionKey(document.PartitionKey),
                    new ItemRequestOptions { },
                    cancellationToken)
                .GetResourceWithEtag<T>(serializer);

        public Task<T> ReplaceAsync(
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
                    },
                    cancellationToken)
                .GetResourceWithEtag<T>(serializer);

        public Task<T> UpdateAsync(
            string documentId,
            string partitionKey,
            Action<T> updateDocument,
            int retries = 0,
            CancellationToken cancellationToken = default)
            => UpdateAsync(
                documentId,
                partitionKey,
                d => MakeAsync(() => updateDocument(d)),
                retries,
                cancellationToken);

        public async Task<T> UpdateAsync(
            string documentId,
            string partitionKey,
            Func<T, Task> updateDocument,
            int retries = 0,
            CancellationToken cancellationToken = default)
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    var document = await reader
                        .ReadAsync(
                            documentId,
                            partitionKey,
                            cancellationToken)
                        .ConfigureAwait(false);

                    await updateDocument(document)
                        .ConfigureAwait(false);

                    return await
                        ReplaceAsync(
                            document,
                            cancellationToken)
                        .ConfigureAwait(false);
                }
                catch (CosmosException ex)
                 when (ex.StatusCode == HttpStatusCode.PreconditionFailed)
                {
                    if (--retries <= 0)
                    {
                        throw;
                    }
                }
            }
        }

        public Task<T> UpdateOrCreateAsync(
            Func<T> getDefaultDocument,
            Action<T> updateDocument,
            int retries = 0,
            CancellationToken cancellationToken = default)
            => UpdateOrCreateAsync(
                getDefaultDocument,
                d => MakeAsync(() => updateDocument(d)),
                retries,
                cancellationToken);

        [SuppressMessage(
            "Major Code Smell",
            "S4457:Parameter validation in \"async\"/\"await\" methods should be wrapped",
            Justification = "By design")]
        public async Task<T> UpdateOrCreateAsync(
            Func<T> getDefaultDocument,
            Func<T, Task> updateDocument,
            int retries = 0,
            CancellationToken cancellationToken = default)
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    var defaultDocument = getDefaultDocument();
                    if (string.IsNullOrEmpty(defaultDocument.DocumentId) ||
                        string.IsNullOrEmpty(defaultDocument.PartitionKey))
                    {
                        throw new ArgumentException(
                            $"Default document needs {nameof(defaultDocument.DocumentId)} " +
                            $"and {nameof(defaultDocument.PartitionKey)} to be set.",
                            nameof(getDefaultDocument));
                    }

                    var document = await reader
                        .FindAsync(
                            defaultDocument.DocumentId,
                            defaultDocument.PartitionKey,
                            cancellationToken)
                        .ConfigureAwait(false)
                        ?? defaultDocument;

                    await updateDocument(document)
                        .ConfigureAwait(false);

                    if (document.ETag is null)
                    {
                        return await CreateAsync(
                            document,
                            cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        return await ReplaceAsync(
                            document,
                            cancellationToken).ConfigureAwait(false);
                    }
                }
                catch (CosmosException ex)
                 when (ex.StatusCode == HttpStatusCode.PreconditionFailed ||
                       ex.StatusCode == HttpStatusCode.Conflict)
                {
                    if (--retries <= 0)
                    {
                        throw;
                    }
                }
            }
        }

        private static Task MakeAsync(Action action)
        {
            action();
            return Task.CompletedTask;
        }
    }
}