namespace Atc.Cosmos.Internal;

/// <summary>
/// Responsible for initializing cosmos database and containers doing aspnet core startup
/// before the API is serving requests.
/// </summary>
public class StartupInitializationJob(ICosmosInitializer initializer) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
        => initializer.InitializeAsync(cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}