#if PREVIEW
namespace Atc.Cosmos
{
    /// <summary>
    /// Represents a factory for creating <see cref="ICosmosWriter{T}"/> instances.
    /// </summary>
    public interface ILowPriorityCosmosWriterFactory
    {
        /// <summary>
        /// Create a <see cref="ICosmosWriter{T}"/>.
        /// </summary>
        /// <typeparam name="TResource">The <see cref="ICosmosResource"/> for the <see cref="ICosmosWriter{T}"/>.</typeparam>
        /// <returns>A <see cref="ICosmosWriter{T}"/>.</returns>
        ILowPriorityCosmosWriter<TResource> CreateWriter<TResource>()
            where TResource : class, ICosmosResource;

        /// <summary>
        /// Create a <see cref="ICosmosBulkWriter{T}"/>.
        /// </summary>
        /// <typeparam name="TResource">The <see cref="ICosmosResource"/> for the <see cref="ICosmosBulkWriter{T}"/>.</typeparam>
        /// <returns>A <see cref="ICosmosBulkWriter{T}"/>.</returns>
        ILowPriorityCosmosBulkWriter<TResource> CreateBulkWriter<TResource>()
            where TResource : class, ICosmosResource;
    }
}
#endif