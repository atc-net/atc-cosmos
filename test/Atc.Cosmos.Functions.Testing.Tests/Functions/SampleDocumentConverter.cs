namespace Atc.Cosmos.Functions.Testing.Tests.Functions;

public sealed class SampleDocumentConverter(JsonSerializerOptions options) : IInputConverter
{
    public ValueTask<ConversionResult> ConvertAsync(ConverterContext context)
    {
        try
        {
            var data = JsonSerializer.Deserialize<List<SampleDocument>>(context.Source!.ToString()!, options);
            return ValueTask.FromResult(ConversionResult.Success(data));
        }
        catch (JsonException ex)
        {
            return ValueTask.FromResult(ConversionResult.Failed(ex));
        }
    }
}