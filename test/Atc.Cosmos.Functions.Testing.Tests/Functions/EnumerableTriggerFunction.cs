namespace Atc.Cosmos.Functions.Testing.Tests.Functions;

public sealed class EnumerableTriggerFunction
{
    public const string Name = nameof(EnumerableTriggerFunction);

    [Function(Name)]
    public Task Run(
        [CosmosDBTrigger("database", "container")] IEnumerable<SampleDocument> input)
        => Task.CompletedTask;
}