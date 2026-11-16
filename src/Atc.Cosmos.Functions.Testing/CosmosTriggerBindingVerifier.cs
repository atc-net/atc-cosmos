namespace Atc.Cosmos.Functions.Testing;

/// <summary>
/// Verifies that Azure Functions Cosmos DB triggers can read the documents Atc.Cosmos writes.
/// </summary>
/// <remarks>
/// For each trigger, a sample document is written as a change feed batch with the Atc.Cosmos serializer
/// (<see cref="CosmosOptions.SerializerOptions"/>), and read the way the Functions worker reads it: with the
/// <see cref="InputConverterAttribute"/> converter on the parameter when there is one, otherwise with
/// <see cref="WorkerOptions.Serializer"/>. The verification fails when reading throws, or when the documents
/// read differ from the documents written.
/// </remarks>
public sealed class CosmosTriggerBindingVerifier
{
    private readonly ServiceProvider services;
    private readonly Func<Type, object> sampleFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="CosmosTriggerBindingVerifier"/> class.
    /// </summary>
    /// <param name="configureServices">
    /// Registers the application's services on top of the Functions worker defaults
    /// (<c>AddFunctionsWorkerDefaults</c>): at least the <see cref="CosmosOptions"/>, the <see cref="WorkerOptions"/>
    /// and the dependencies of the input converters.
    /// </param>
    /// <param name="sampleFactory">Creates a sample document of the given type, for example with AutoFixture.</param>
    public CosmosTriggerBindingVerifier(
        Action<IServiceCollection> configureServices,
        Func<Type, object> sampleFactory)
    {
        ArgumentNullException.ThrowIfNull(configureServices);
        ArgumentNullException.ThrowIfNull(sampleFactory);

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddFunctionsWorkerDefaults();
        configureServices(serviceCollection);

        services = serviceCollection.BuildServiceProvider();
        this.sampleFactory = sampleFactory;
    }

    /// <summary>
    /// Verifies the Cosmos DB trigger of the function with the given name.
    /// </summary>
    /// <param name="assembly">The assembly with the functions.</param>
    /// <param name="functionName">The name given in the <see cref="FunctionAttribute"/>.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the verification.</returns>
    /// <exception cref="ArgumentException">The assembly has no function with that name and a Cosmos DB trigger.</exception>
    public Task<CosmosTriggerBindingResult> VerifyAsync(
        Assembly assembly,
        string functionName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        var binding = CosmosTriggerDiscovery
            .Find(assembly)
            .FirstOrDefault(x => string.Equals(x.FunctionName, functionName, StringComparison.Ordinal))
            ?? throw new ArgumentException(
                $"The assembly '{assembly.GetName().Name}' has no function '{functionName}' with a Cosmos DB trigger.",
                nameof(functionName));

        return VerifyAsync(binding, cancellationToken);
    }

    /// <summary>
    /// Verifies the given Cosmos DB trigger.
    /// </summary>
    /// <param name="binding">The trigger, as found by <see cref="CosmosTriggerDiscovery.Find"/>.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the verification.</returns>
    public async Task<CosmosTriggerBindingResult> VerifyAsync(
        CosmosTriggerBinding binding,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(binding);

        var cosmosSerializer = new JsonCosmosSerializer(services.GetRequiredService<IOptions<CosmosOptions>>());
        var writtenJson = Write(cosmosSerializer, CreateBatch(binding.DocumentType));
        var documentTypeName = binding.DocumentType.Name;

        object? documents;
        try
        {
            documents = await ReadAsync(binding, writtenJson, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            var failedMessage =
                $"{binding.FunctionName} cannot read {documentTypeName} documents written by Atc.Cosmos: {ex.Message} " +
                "Set WorkerOptions.Serializer to the Cosmos serializer options, or add an [InputConverter] to the trigger parameter.";

            return new CosmosTriggerBindingResult(
                binding.FunctionName,
                binding.DocumentType,
                IsSuccess: false,
                failedMessage,
                writtenJson,
                ex);
        }

        var readJson = documents is null
            ? "null"
            : Write(cosmosSerializer, documents);

        if (!JsonNode.DeepEquals(JsonNode.Parse(writtenJson), JsonNode.Parse(readJson)))
        {
            var differentMessage =
                $"{binding.FunctionName} reads {documentTypeName} documents written by Atc.Cosmos with different values. " +
                $"Written: {writtenJson} Read: {readJson} " +
                "Use the Cosmos serializer options for WorkerOptions.Serializer or in the [InputConverter] of the trigger parameter.";

            return new CosmosTriggerBindingResult(
                binding.FunctionName,
                binding.DocumentType,
                IsSuccess: false,
                differentMessage,
                writtenJson,
                Exception: null);
        }

        return new CosmosTriggerBindingResult(
            binding.FunctionName,
            binding.DocumentType,
            IsSuccess: true,
            $"{binding.FunctionName} reads {documentTypeName} documents written by Atc.Cosmos.",
            writtenJson,
            Exception: null);
    }

    private object CreateBatch(Type documentType)
    {
        var batch = (System.Collections.IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(documentType))!;
        batch.Add(sampleFactory(documentType));

        return batch;
    }

    private static string Write(
        JsonCosmosSerializer cosmosSerializer,
        object documents)
    {
        using var stream = cosmosSerializer.ToStream(documents);
        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }

    private async Task<object?> ReadAsync(
        CosmosTriggerBinding binding,
        string writtenJson,
        CancellationToken cancellationToken)
    {
        if (binding.InputConverterType is { } converterType)
        {
            var converter = (IInputConverter)ActivatorUtilities.CreateInstance(services, converterType);
            var result = await converter
                .ConvertAsync(new VerifierConverterContext(binding.Parameter.ParameterType, writtenJson))
                .ConfigureAwait(false);

            return result.Status switch
            {
                ConversionStatus.Succeeded => result.Value,
                ConversionStatus.Failed => throw result.Error ?? new InvalidOperationException($"The input converter '{converterType.Name}' failed without an exception."),
                _ => throw new InvalidOperationException($"The input converter '{converterType.Name}' did not handle the change feed batch."),
            };
        }

        var serializer = services.GetRequiredService<IOptions<WorkerOptions>>().Value.Serializer
            ?? throw new InvalidOperationException("WorkerOptions.Serializer is not set.");

        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(writtenJson));

        return await serializer
            .DeserializeAsync(stream, binding.Parameter.ParameterType, cancellationToken)
            .ConfigureAwait(false);
    }
}