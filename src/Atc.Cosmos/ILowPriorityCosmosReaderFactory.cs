#if PREVIEW
namespace Atc.Cosmos
{
    /// <summary>
    /// Represents a factory for creating <see cref="ICosmosReader{T}"/> instances.
    /// </summary>
    public interface ILowPriorityCosmosReaderFactory
    {
        /// <summary>
        /// Create a <see cref="ICosmosReader{T}"/>.
        /// </summary>
        /// <typeparam name="TResource">The <see cref="ICosmosResource"/> for the <see cref="ICosmosReader{T}"/>.</typeparam>
        /// <returns>A <see cref="ICosmosReader{T}"/>.</returns>
        ILowPriorityCosmosReader<TResource> CreateReader<TResource>()
            where TResource : class, ICosmosResource;

        /// <summary>
        /// Create a <see cref="ICosmosBulkReader{T}"/>.
        /// </summary>
        /// <typeparam name="TResource">The <see cref="ICosmosResource"/> for the <see cref="ICosmosBulkReader{T}"/>.</typeparam>
        /// <returns>A <see cref="ICosmosBulkReader{T}"/>.</returns>
        ILowPriorityCosmosBulkReader<TResource> CreateBulkReader<TResource>()
            where TResource : class, ICosmosResource;
    }
}
#endif