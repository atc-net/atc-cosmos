namespace Atc.Cosmos.AutoIncrement;

public class AutoIncrementProvider(
    ICosmosWriter<AutoIncrementCounter> writer)
    : IAutoIncrementProvider
{
    public async Task<int> GetNextAsync(
        string counterName,
        CancellationToken cancellationToken)
    {
        var result = await writer
            .UpdateOrCreateAsync(
                () => new AutoIncrementCounter
                {
                    CounterName = counterName,
                },
                d => d.Count++,
                retries: 5,
                cancellationToken)
            .ConfigureAwait(false);

        return result.Count;
    }
}