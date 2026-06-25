namespace Atc.Cosmos.Internal;

public class ChangeFeedService(IEnumerable<IChangeFeedListener> listeners)
    : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        var tasks = listeners
            .Select(l => l.StartAsync(cancellationToken))
            .ToArray();

        return Task.WhenAll(tasks);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        var tasks = listeners
            .Select(l => l.StopAsync(cancellationToken))
            .ToArray();

        return Task.WhenAll(tasks);
    }
}