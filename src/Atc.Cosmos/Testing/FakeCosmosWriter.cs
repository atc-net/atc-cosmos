using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace Atc.Cosmos.Testing
{
    /// <summary>
    /// Represents a fake <see cref="ICosmosWriter{T}"/>
    /// or <see cref="ICosmosBulkWriter{T}"/> that can be
    /// used when unit testing client code.
    /// </summary>
    /// <typeparam name="T">
    /// The type of <see cref="ICosmosResource"/>
    /// to be read by this reader.
    /// </typeparam>
    [SuppressMessage(
       "Design",
       "MA0016:Prefer return collection abstraction instead of implementation",
       Justification = "By design")]
    [SuppressMessage(
       "Design",
       "CA1002:Do not expose generic lists",
       Justification = "By design")]
    [SuppressMessage(
       "Usage",
       "CA2227:Collection properties should be read only",
       Justification = "By design")]
    public class FakeCosmosWriter<T> :
        ICosmosWriter<T>,
        ICosmosBulkWriter<T>
        where T : class, ICosmosResource
    {
        private readonly JsonSerializerOptions? options;

        public FakeCosmosWriter()
        {
        }

        public FakeCosmosWriter(JsonSerializerOptions options)
        {
            this.options = options;
        }

        /// <summary>
        /// Gets or sets the list of documents to be modified by the fake writer.
        /// </summary>
        public List<T> Documents { get; set; }
            = new List<T>();

        public virtual Task<T> CreateAsync(
            T document,
            CancellationToken cancellationToken = default)
        {
            GuardNotExists(document);

            T newDocument = document.Clone(options);
            newDocument.ETag = Guid.NewGuid().ToString();
            Documents.Add(newDocument);
            return Task.FromResult(newDocument);
        }

        public virtual Task<T> WriteAsync(
            T document,
            CancellationToken cancellationToken = default)
        {
            Documents.RemoveAll(d
                => d.DocumentId == document.DocumentId
                && d.PartitionKey == document.PartitionKey);

            var newDocument = document.Clone(options);
            newDocument.ETag = Guid.NewGuid().ToString();
            Documents.Add(newDocument);

            return Task.FromResult(newDocument);
        }

        public virtual Task<T> ReplaceAsync(
            T document,
            CancellationToken cancellationToken = default)
        {
            GuardExistsWithEtag(document);

            Documents.RemoveAll(d
                => d.DocumentId == document.DocumentId
                && d.PartitionKey == document.PartitionKey);

            var newDocument = document.Clone(options);
            newDocument.ETag = Guid.NewGuid().ToString();
            Documents.Add(newDocument);

            return Task.FromResult(newDocument);
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

        public virtual Task<T> UpdateAsync(
            string documentId,
            string partitionKey,
            Func<T, Task> updateDocument,
            int retries = 0,
            CancellationToken cancellationToken = default)
        {
            var document = GuardExists(documentId, partitionKey);

            var newDocument = document.Clone(options);
            updateDocument(newDocument);
            newDocument.ETag = Guid.NewGuid().ToString();

            Documents.Remove(document);
            Documents.Add(newDocument);

            return Task.FromResult(newDocument);
        }

        public virtual Task<T> UpdateAsync(
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

        public virtual Task<T> UpdateOrCreateAsync(
            Func<T> getDefaultDocument,
            Func<T, Task> updateDocument,
            int retries = 0,
            CancellationToken cancellationToken = default)
        {
            var defaultDocument = getDefaultDocument();
            var existingDocument = Documents.Find(d
                => d.DocumentId == defaultDocument.DocumentId
                && d.PartitionKey == defaultDocument.PartitionKey);

            var newDocument = (existingDocument ?? defaultDocument).Clone(options);
            updateDocument(newDocument);

            newDocument.ETag = Guid.NewGuid().ToString();
            if (existingDocument is not null)
            {
                Documents.Remove(existingDocument);
            }

            Documents.Add(newDocument);

            return Task.FromResult(newDocument);
        }

        public virtual Task<T> UpdateOrCreateAsync(
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

        Task ICosmosBulkWriter<T>.CreateAsync(
            T document,
            CancellationToken cancellationToken)
            => CreateAsync(document, cancellationToken);

        Task ICosmosBulkWriter<T>.WriteAsync(
            T document,
            CancellationToken cancellationToken)
            => WriteAsync(document, cancellationToken);

        Task ICosmosBulkWriter<T>.ReplaceAsync(
            T document,
            CancellationToken cancellationToken)
            => ReplaceAsync(document, cancellationToken);

        protected void GuardNotExists(
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

        protected void GuardExistsWithEtag(ICosmosResource document)
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

        protected T GuardExists(ICosmosResource document)
            => GuardExists(document.DocumentId, document.PartitionKey);

        protected T GuardExists(
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