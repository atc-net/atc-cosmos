using System.Diagnostics.CodeAnalysis;
using System.IO;
using Atc.Cosmos.Serialization;
using Microsoft.Azure.Cosmos;

namespace Atc.Cosmos.Internal
{
    public class CosmosSerializerAdapter : CosmosSerializer
    {
        public CosmosSerializerAdapter(IJsonCosmosSerializer serializer)
        {
            Serializer = serializer;
        }

        public IJsonCosmosSerializer Serializer { get; }

        [return: MaybeNull]
        public override T FromStream<T>(Stream stream)
            => Serializer.FromStream<T>(stream);

        public override Stream ToStream<T>(T input)
            => Serializer.ToStream(input);
    }
}