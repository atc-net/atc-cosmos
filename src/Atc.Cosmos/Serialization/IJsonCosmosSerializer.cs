namespace Atc.Cosmos.Serialization;

public interface IJsonCosmosSerializer
{
    T? FromStream<T>(Stream stream);

    Stream ToStream<T>(T input);

    string SerializeMemberName(MemberInfo memberInfo);

    T? FromString<T>(string json);
}