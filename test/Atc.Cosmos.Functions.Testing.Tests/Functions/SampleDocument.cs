namespace Atc.Cosmos.Functions.Testing.Tests.Functions;

public sealed class SampleDocument
{
    public string Id { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public SampleDocumentType Type { get; set; }
}