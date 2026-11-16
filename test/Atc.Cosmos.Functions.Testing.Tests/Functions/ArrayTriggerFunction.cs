namespace Atc.Cosmos.Functions.Testing.Tests.Functions;

public sealed class ArrayTriggerFunction
{
    public const string Name = nameof(ArrayTriggerFunction);

    [Function(Name)]
    public Task Run(
        [CosmosDBTrigger("database", "container")] SampleDocument[] input)
        => Task.CompletedTask;
}