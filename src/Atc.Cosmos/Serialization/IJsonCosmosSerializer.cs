namespace Atc.Cosmos.Serialization;

public interface IJsonCosmosSerializer
{
    [return: MaybeNull]
    T FromStream<T>(Stream stream);

    Stream ToStream<T>(T input);

    string SerializeMemberName(MemberInfo memberInfo);

    [return: MaybeNull]
    T FromString<T>(string json);
}