using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Atc.Cosmos.Internal
{
    /// <summary>
    /// Implementation used for serializing a stream to and from Json using the <seealso cref="System.Text.Json.JsonSerializer"/>
    /// from within Cosmos SDK.
    /// </summary>
    public class JsonCosmosSerializer : IJsonCosmosSerializer
    {
        private readonly JsonSerializerOptions options;

        public JsonCosmosSerializer(IOptions<CosmosOptions> options)
        {
            this.options = options.Value.SerializerOptions;
        }

        [return: MaybeNull]
        public T FromStream<T>(Stream stream)
        {
            if (stream is null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            using (stream)
            {
                if (stream.CanSeek && stream.Length == 0)
                {
                    return default;
                }

                // This part is taken from one of the Cosmos samples.
                if (typeof(Stream).IsAssignableFrom(typeof(T)))
                {
                    return (T)(object)stream;
                }

                // Response data from cosmos always comes as a memory stream.
                // Note: This might change in v4, but so far it doesn't look like it.
                if (stream is MemoryStream memoryStream && memoryStream.TryGetBuffer(out ArraySegment<byte> buffer))
                {
                    return JsonSerializer.Deserialize<T>(buffer, options);
                }

                return default;
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

        [return: MaybeNull]
        public T FromString<T>(string json)
            => JsonSerializer.Deserialize<T>(
                json,
                options);
    }
}