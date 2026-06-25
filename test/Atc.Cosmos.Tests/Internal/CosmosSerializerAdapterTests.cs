namespace Atc.Cosmos.Tests.Internal;

public sealed class CosmosSerializerAdapterTests
{
    [Theory, AutoNSubstituteData]
    public void FromStream_Delegates_To_Serializer(
        IJsonCosmosSerializer serializer,
        Record record)
    {
        // Arrange
        using var stream = new MemoryStream();

        serializer
            .FromStream<Record>(stream)
            .Returns(record);

        var sut = new CosmosSerializerAdapter(serializer);

        // Act
        var result = sut.FromStream<Record>(stream);

        // Assert
        result.Should().Be(record);

        serializer
            .Received(1)
            .FromStream<Record>(stream);
    }

    [Theory, AutoNSubstituteData]
    public void ToStream_Delegates_To_Serializer(
        IJsonCosmosSerializer serializer,
        Record record)
    {
        // Arrange
        using var stream = new MemoryStream();

        serializer
            .ToStream(record)
            .Returns(stream);

        var sut = new CosmosSerializerAdapter(serializer);

        // Act
        var result = sut.ToStream(record);

        // Assert
        result.Should().BeSameAs(stream);

        serializer
            .Received(1)
            .ToStream(record);
    }

    [Theory, AutoNSubstituteData]
    public void SerializeMemberName_Delegates_To_Serializer(
        IJsonCosmosSerializer serializer,
        string name)
    {
        // Arrange
        var memberInfo = typeof(Record).GetProperty(nameof(Record.Id))!;

        serializer
            .SerializeMemberName(memberInfo)
            .Returns(name);

        var sut = new CosmosSerializerAdapter(serializer);

        // Act
        var result = sut.SerializeMemberName(memberInfo);

        // Assert
        result.Should().Be(name);

        serializer
            .Received(1)
            .SerializeMemberName(memberInfo);
    }

    [Theory, AutoNSubstituteData]
    public void Exposes_The_Injected_Serializer(
        IJsonCosmosSerializer serializer)
    {
        // Arrange
        var sut = new CosmosSerializerAdapter(serializer);

        // Act & assert
        sut.Serializer.Should().BeSameAs(serializer);
    }
}