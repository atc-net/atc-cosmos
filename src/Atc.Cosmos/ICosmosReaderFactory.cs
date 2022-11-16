namespace Atc.Cosmos
{
    /// <summary>
    /// Represents a factory for creating <see cref="ICosmosReader{T}"/> instances.
    /// </summary>
    public interface ICosmosReaderFactory
    {
        /// <summary>
        /// Create a <see cref="ICosmosReader{T}"/>.
        /// </summary>
        /// <typeparam name="TResource">The <see cref="ICosmosResource"/> for the <see cref="ICosmosReader{T}"/>.</typeparam>
        /// <returns>A <see cref="ICosmosReader{T}"/>.</returns>
        ICosmosReader<TResource> CreateReader<TResource>()
            where TResource : class, ICosmosResource;

        /// <summary>
        /// Create a <see cref="ICosmosBulkReader{T}"/>.
        /// </summary>
        /// <typeparam name="TResource">The <see cref="ICosmosResource"/> for the <see cref="ICosmosBulkReader{T}"/>.</typeparam>
        /// <returns>A <see cref="ICosmosBulkReader{T}"/>.</returns>
        ICosmosBulkReader<TResource> CreateBulkReader<TResource>()
            where TResource : class, ICosmosResource;

        /// <summary>
        /// Create a <see cref="ICosmosBatchReader{T}"/>.
        /// </summary>
        /// <typeparam name="TResource">The <see cref="ICosmosResource"/> for the <see cref="ICosmosBulkReader{T}"/>.</typeparam>
        /// <returns>A <see cref="CreateBatchReader{TResource}"/>.</returns>
        ICosmosBatchReader<TResource> CreateBatchReader<TResource>()
            where TResource : class, ICosmosResource;
    }
}
