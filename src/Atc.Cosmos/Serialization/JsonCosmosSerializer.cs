namespace Atc.Cosmos.Serialization;

/// <summary>
/// Implementation used for serializing a stream to and from Json using the <seealso cref="System.Text.Json.JsonSerializer"/>
/// from within Cosmos SDK.
/// </summary>
public class JsonCosmosSerializer(
    IOptions<CosmosOptions> options)
    : IJsonCosmosSerializer
{
    private readonly JsonSerializerOptions options = options.Value.SerializerOptions;

    public T? FromStream<T>(Stream stream)
    {
        if (stream is null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        using (stream)
        {
            if (stream is { CanSeek: true, Length: 0 })
            {
                return default;
            }

            // This part is taken from one of the Cosmos samples.
            if (typeof(Stream).IsAssignableFrom(typeof(T)))
            {
                return (T)(object)stream;
            }

            // Fast path: response data from cosmos usually comes as a memory
            // stream whose buffer can be read directly without copying.
            if (stream is MemoryStream memoryStream && memoryStream.TryGetBuffer(out var buffer))
            {
                return JsonSerializer.Deserialize<T>(buffer, options);
            }

            // The Cosmos SDK does not guarantee the response is a MemoryStream
            // with a publicly visible buffer (e.g. the Binary Encoding feature
            // added in newer SDK versions wraps it), so fall back to copying
            // the stream into a buffer before deserializing.
            using var copy = new MemoryStream();
            stream.CopyTo(copy);

            return JsonSerializer.Deserialize<T>(
                new ReadOnlySpan<byte>(copy.GetBuffer(), 0, (int)copy.Length),
                options);
        }
    }

    public Stream ToStream<T>(T input)
    {
        if (input is null)
        {
            throw new ArgumentNullException(nameof(input));
        }

        var streamPayload = new MemoryStream();

        using var utf8JsonWriter = new Utf8JsonWriter(
            streamPayload,
            new JsonWriterOptions
            {
                Indented = options.WriteIndented,
            });

        JsonSerializer.Serialize(utf8JsonWriter, input, options);
        streamPayload.Position = 0;

        return streamPayload;
    }

    public string SerializeMemberName(MemberInfo memberInfo)
        => options.PropertyNamingPolicy?.ConvertName(memberInfo.Name) ??
           memberInfo.Name;

    public T? FromString<T>(string json)
        => JsonSerializer.Deserialize<T>(
            json,
            options);
}