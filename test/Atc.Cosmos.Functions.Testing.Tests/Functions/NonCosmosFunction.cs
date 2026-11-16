namespace Atc.Cosmos.Functions.Testing.Tests.Functions;

public sealed class NonCosmosFunction
{
    public const string Name = nameof(NonCosmosFunction);

    [Function(Name)]
    public Task Run(string input)
        => Task.CompletedTask;
}