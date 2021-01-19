using System.Threading;
using System.Threading.Tasks;

namespace Atc.Cosmos.AutoIncrement
{
    public class AutoIncrementProvider : IAutoIncrementProvider
    {
        private readonly ICosmosWriter<AutoIncrementCounter> writer;

        public AutoIncrementProvider(
            ICosmosWriter<AutoIncrementCounter> writer)
        {
            this.writer = writer;
        }

        public async Task<int> GetNextAsync(string counterName, CancellationToken cancellationToken)
        {
            var result = await writer.UpdateOrCreateAsync(
                () => new AutoIncrementCounter
                {
                    CounterName = counterName,
                },
                d => d.Count++,
                retries: 5,
                cancellationToken);

            return result.Count;
        }
    }
}