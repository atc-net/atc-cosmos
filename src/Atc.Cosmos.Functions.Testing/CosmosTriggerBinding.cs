namespace Atc.Cosmos.Functions.Testing;

/// <summary>
/// An Azure Functions method with a Cosmos DB trigger parameter.
/// </summary>
/// <param name="FunctionName">The name given in the <see cref="FunctionAttribute"/>.</param>
/// <param name="Method">The function method.</param>
/// <param name="Parameter">The parameter with the <see cref="CosmosDBTriggerAttribute"/>.</param>
/// <param name="DocumentType">The type of the documents in the change feed batch.</param>
/// <param name="InputConverterType">The converter given in an <see cref="InputConverterAttribute"/> on the parameter, if any.</param>
public sealed record CosmosTriggerBinding(
    string FunctionName,
    MethodInfo Method,
    ParameterInfo Parameter,
    Type DocumentType,
    Type? InputConverterType);