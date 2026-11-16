namespace Atc.Cosmos.Functions.Testing.Tests.Functions;

public sealed class InputConverterTriggerFunction
{
    public const string Name = nameof(InputConverterTriggerFunction);

    [Function(Name)]
    public Task Run(
        [CosmosDBTrigger("database", "container")]
        [InputConverter(typeof(SampleDocumentConverter))] IReadOnlyList<SampleDocument> input)
        => Task.CompletedTask;
}