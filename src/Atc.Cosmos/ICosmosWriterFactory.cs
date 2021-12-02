namespace Atc.Cosmos
{
    /// <summary>
    /// Represents a factory for creating <see cref="ICosmosWriter{T}"/> instances.
    /// </summary>
    public interface ICosmosWriterFactory
    {
        /// <summary>
        /// Create a <see cref="ICosmosWriter{T}"/>.
        /// </summary>
        /// <typeparam name="TResource">The <see cref="ICosmosResource"/> for the <see cref="ICosmosWriter{T}"/>.</typeparam>
        /// <returns>A <see cref="ICosmosWriter{T}"/>.</returns>
        ICosmosWriter<TResource> CreateWriter<TResource>()
            where TResource : class, ICosmosResource;

        /// <summary>
        /// Create a <see cref="ICosmosBulkWriter{T}"/>.
        /// </summary>
        /// <typeparam name="TResource">The <see cref="ICosmosResource"/> for the <see cref="ICosmosBulkWriter{T}"/>.</typeparam>
        /// <returns>A <see cref="ICosmosBulkWriter{T}"/>.</returns>
        ICosmosBulkWriter<TResource> CreateBulkWriter<TResource>()
            where TResource : class, ICosmosResource;
    }
}
