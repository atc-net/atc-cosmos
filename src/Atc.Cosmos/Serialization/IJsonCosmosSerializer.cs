using System.IO;

namespace Atc.Cosmos.Serialization
{
    public interface IJsonCosmosSerializer
    {
        T FromStream<T>(Stream stream);

        Stream ToStream<T>(T input);

        T FromString<T>(string json);
    }
}