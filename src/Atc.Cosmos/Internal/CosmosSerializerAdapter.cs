namespace Atc.Cosmos.Internal;

public class CosmosSerializerAdapter(
    IJsonCosmosSerializer serializer)
    : CosmosLinqSerializer
{
    public IJsonCosmosSerializer Serializer { get; } = serializer;

    // CosmosSerializer.FromStream<T> returns plain T, so the override must
    // keep [MaybeNull] rather than T? (an unconstrained T? would be read as
    // Nullable<T> and fail to match the base signature).
    [return: MaybeNull]
    public override T FromStream<T>(Stream stream)
        => Serializer.FromStream<T>(stream);

    public override Stream ToStream<T>(T input)
        => Serializer.ToStream(input);

    public override string SerializeMemberName(MemberInfo memberInfo)
        => Serializer.SerializeMemberName(memberInfo);
}