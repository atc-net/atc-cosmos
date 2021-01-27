using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Atc.Cosmos.Serialization
{
    public interface IJsonCosmosSerializer
    {
        [return: MaybeNull]
        T FromStream<T>(Stream stream);

        Stream ToStream<T>(T input);

        [return: MaybeNull]
        T FromString<T>(string json);
    }
}