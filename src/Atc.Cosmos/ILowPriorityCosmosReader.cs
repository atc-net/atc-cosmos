#if PREVIEW
namespace Atc.Cosmos
{
    /// <summary>
    /// Represents a reader that can read Cosmos resources using the PriorityLevel Low.
    /// </summary>
    /// <typeparam name="T">
    /// The type of <see cref="ICosmosResource"/>
    /// to be read by this reader.
    /// </typeparam>
    public interface ILowPriorityCosmosReader<T>
        : ICosmosReader<T>
        where T : class, ICosmosResource
    {
    }
}
#endif