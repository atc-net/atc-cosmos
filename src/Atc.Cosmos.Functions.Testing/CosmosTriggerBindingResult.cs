namespace Atc.Cosmos.Functions.Testing;

/// <summary>
/// The result of verifying that a Cosmos DB trigger can read the documents Atc.Cosmos writes.
/// </summary>
/// <param name="FunctionName">The name of the function.</param>
/// <param name="DocumentType">The type of the documents in the change feed batch.</param>
/// <param name="IsSuccess"><see langword="true"/> when the trigger reads the documents with the values they were written with.</param>
/// <param name="Message">A description of the result, which names the cause and the fix when the verification fails.</param>
/// <param name="WrittenJson">The change feed batch, written like Atc.Cosmos writes documents.</param>
/// <param name="Exception">The exception the trigger would throw when it reads the batch, if any.</param>
public sealed record CosmosTriggerBindingResult(
    string FunctionName,
    Type DocumentType,
    bool IsSuccess,
    string Message,
    string WrittenJson,
    Exception? Exception);