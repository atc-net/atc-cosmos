#if PREVIEW
namespace Atc.Cosmos
{
    /// <summary>
    /// Represents a writer that can write Cosmos resources using the PriorityLevel Low.
    /// </summary>
    /// <typeparam name="T">
    /// The type of <see cref="ICosmosResource"/>
    /// to be read by this reader.
    /// </typeparam>
    public interface ILowPriorityCosmosWriter<T>
        : ICosmosWriter<T>
        where T : class, ICosmosResource
    {
    }
}
#endif