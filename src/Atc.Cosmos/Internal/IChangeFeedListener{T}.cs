namespace Atc.Cosmos.Internal
{
    public interface IChangeFeedListener<T> : IChangeFeedListener
        where T : class, ICosmosResource
    {
    }
}
