namespace Atc.Cosmos.Tests.Serialization;

public sealed class JsonCosmosSerializerTests
{
    private readonly JsonCosmosSerializer sut;

    public JsonCosmosSerializerTests()
    {
        var options = new OptionsWrapper<CosmosOptions>(
            new CosmosOptions
            {
                SerializerOptions = new JsonSerializerOptions(),
            });

        sut = new JsonCosmosSerializer(options);
    }

    [Fact]
    public void ToStream_ShouldThrow_When_Stream_IsNull()
        => FluentActions.Invoking(() => sut.ToStream<Record>(input: null))
            .Should()
            .Throw<ArgumentNullException>();

    [Fact]
    public void ToStream_ShouldThrow_When_Object_IsNull()
        => FluentActions.Invoking(() => sut.ToStream<Record>(input: null))
            .Should()
            .Throw<ArgumentNullException>();

    [Theory, AutoNSubstituteData]
    public void ToStream_Should_Provide_MemoryStream(Record typedObject)
        => sut
            .ToStream(typedObject)
            .Should()
            .BeOfType<MemoryStream>();

    [Theory, AutoNSubstituteData]
    public void ToStream_Should_Have_StartPosition_Zero_InStream(
        Record typedObject)
        => sut
            .ToStream(typedObject)
            .Position
            .Should()
            .Be(0);

    [Theory, AutoNSubstituteData]
    public void ToStream_Should_Have_Content(Record typedObject)
        => sut
            .ToStream(typedObject)
            .Length
            .Should()
            .BeGreaterThan(0);

    [Theory, AutoNSubstituteData]
    public void FromStream_Should_Return_TypedObject(Record typedObject)
    {
        // Arrange
        using var stream = sut.ToStream(typedObject);

        // Act & assert
        sut.FromStream<Record>(stream).Should().BeEquivalentTo(typedObject);
    }

    [Theory, AutoNSubstituteData]
    public void FromStream_Should_Return_TypedObject_When_Buffer_Is_Not_Exposable(
        Record typedObject)
    {
        // Arrange (newer Cosmos SDK versions return a stream whose buffer is
        // not publicly visible, e.g. the Binary Encoding wrapper)
        using var source = sut.ToStream(typedObject);
        var bytes = ((MemoryStream)source).ToArray();
        using var stream = new MemoryStream(bytes, index: 0, count: bytes.Length, writable: false, publiclyVisible: false);

        // Act & assert
        sut.FromStream<Record>(stream).Should().BeEquivalentTo(typedObject);
    }

    [Fact]
    public void FromStream_ShouldThrow_If_Stream_IsNull()
        => FluentActions.Invoking(() => sut.FromStream<Record>(stream: null))
            .Should()
            .Throw<ArgumentNullException>();

    [Fact]
    public void FromStream_Should_Return_Null_If_Stream_IsEmpty()
        => sut
            .FromStream<Record>(Stream.Null)
            .Should()
            .BeNull();
}