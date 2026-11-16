namespace Atc.Cosmos.Functions.Testing.Tests.Functions;

public static class NotAFunction
{
    public const string Name = nameof(NotAFunction);

    public static Task Run(
        [CosmosDBTrigger("database", "container")] IReadOnlyList<SampleDocument> input)
        => Task.CompletedTask;
}