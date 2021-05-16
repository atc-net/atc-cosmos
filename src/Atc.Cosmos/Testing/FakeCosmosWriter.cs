using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace Atc.Cosmos.Testing
{
    public class FakeCosmosWriter<T> : ICosmosWriter<T>
        where T : class, ICosmosResource
    {
        /// <summary>
        /// Gets or sets the list of documents to be modified by the fake writer.
        /// </summary>
        [SuppressMessage(
            "Design",
            "MA0016:Prefer return collection abstraction instead of implementation",
            Justification = "By design")]
        public List<T> Documents { get; set; }
            = new List<T>();

        public virtual Task<T> CreateAsync(
            T document,
            CancellationToken cancellationToken = default)
        {
            GuardNotExists(document);

            document.ETag = Guid.NewGuid().ToString();
            Documents.Add(document);
            return Task.FromResult(document);
        }

        public Task<T> WriteAsync(
            T document,
            CancellationToken cancellationToken = default)
        {
            Documents.RemoveAll(d
                => d.DocumentId == document.DocumentId
                && d.PartitionKey == document.PartitionKey);

            document.ETag = Guid.NewGuid().ToString();
            Documents.Add(document);

            return Task.FromResult(document);
        }

        public virtual Task<T> ReplaceAsync(
            T document,
            CancellationToken cancellationToken = default)
        {
            GuardExistsWithEtag(document);

            Documents.RemoveAll(d
                => d.DocumentId == document.DocumentId
                && d.PartitionKey == document.PartitionKey);

            document.ETag = Guid.NewGuid().ToString();
            Documents.Add(document);

            return Task.FromResult(document);
        }

        public virtual Task DeleteAsync(
            string documentId,
            string partitionKey,
            CancellationToken cancellationToken = default)
        {
            GuardExists(documentId, partitionKey);

            Documents.RemoveAll(d
                => d.DocumentId == documentId
                && d.PartitionKey == partitionKey);

            return Task.CompletedTask;
        }

        public Task<T> UpdateAsync(
            string documentId,
            string partitionKey,
            Func<T, Task> updateDocument,
            int retries = 0,
            CancellationToken cancellationToken = default)
        {
            var document = GuardExists(documentId, partitionKey);
            updateDocument(document);
            document.ETag = Guid.NewGuid().ToString();

            return Task.FromResult(document);
        }

        public Task<T> UpdateAsync(
            string documentId,
            string partitionKey,
            Action<T> updateDocument,
            int retries = 0,
            CancellationToken cancellationToken = default)
            => UpdateAsync(
                documentId,
                partitionKey,
                d =>
                {
                    updateDocument(d);
                    return Task.CompletedTask;
                },
                retries,
                cancellationToken);

        public Task<T> UpdateOrCreateAsync(
            Func<T> getDefaultDocument,
            Func<T, Task> updateDocument,
            int retries = 0,
            CancellationToken cancellationToken = default)
        {
            var defaultDocument = getDefaultDocument();
            var existingDocument = Documents.Find(d
                => d.DocumentId == defaultDocument.DocumentId
                && d.PartitionKey == defaultDocument.PartitionKey);

            var document = existingDocument ?? defaultDocument;
            updateDocument(document);
            document.ETag = Guid.NewGuid().ToString();

            return Task.FromResult(document);
        }

        public Task<T> UpdateOrCreateAsync(
            Func<T> getDefaultDocument,
            Action<T> updateDocument,
            int retries = 0,
            CancellationToken cancellationToken = default)
            => UpdateOrCreateAsync(
                getDefaultDocument,
                d =>
                {
                    updateDocument(d);
                    return Task.CompletedTask;
                },
                retries,
                cancellationToken);

        private void GuardNotExists(
            ICosmosResource document)
        {
            var existingDocument = Documents.Find(d
                => d.DocumentId == document.DocumentId
                && d.PartitionKey == document.PartitionKey);

            if (existingDocument is not null)
            {
                throw new CosmosException(
                    $"Document already exists.",
                    HttpStatusCode.Conflict,
                    0,
                    string.Empty,
                    0);
            }
        }

        private void GuardExistsWithEtag(ICosmosResource document)
        {
            var existingDocument = GuardExists(document);
            if (existingDocument.ETag != document.ETag)
            {
                throw new CosmosException(
                    $"Document ETag does not match, " +
                    $"indicating incorrecty document version.",
                    HttpStatusCode.PreconditionFailed,
                    0,
                    string.Empty,
                    0);
            }
        }

        private T GuardExists(ICosmosResource document)
            => GuardExists(document.DocumentId, document.PartitionKey);

        private T GuardExists(
            string documentId,
            string partitionKey)
        {
            var item = Documents.Find(d
                => d.DocumentId == documentId
                && d.PartitionKey == partitionKey);

            if (item is null)
            {
                throw new CosmosException(
                    $"Document not found. " +
                    $"Id: {documentId}. " +
                    $"PartitionKey: {partitionKey}",
                    HttpStatusCode.NotFound,
                    0,
                    string.Empty,
                    0);
            }

            return item;
        }
    }
}
