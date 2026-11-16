namespace Atc.Cosmos.Functions.Testing.Tests.Functions;

public sealed class ListTriggerFunction
{
    public const string Name = nameof(ListTriggerFunction);

    [Function(Name)]
    public Task Run(
        [CosmosDBTrigger("database", "container")] IReadOnlyList<SampleDocument> input)
        => Task.CompletedTask;
}