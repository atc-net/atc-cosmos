using System;
using System.IO;
using System.Text.Json;
using Atc.Cosmos.Internal;
using Atc.Test;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;
using static FluentAssertions.FluentActions;

namespace Atc.Cosmos.Tests.Internal
{
    public class JsonCosmosSerializerTests
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
            => Invoking(() => sut.ToStream<Record>(null))
                .Should()
                .Throw<ArgumentNullException>();

        [Fact]
        public void ToStream_ShouldThrow_When_Object_IsNull()
            => Invoking(() => sut.ToStream<Record>(null))
                .Should()
                .Throw<ArgumentNullException>();

        [Theory, AutoNSubstituteData]
        public void ToStream_Should_Provide_MemoryStream(
            Record typedObject)
            => sut.ToStream(typedObject)
                .Should()
                .BeOfType<MemoryStream>();

        [Theory, AutoNSubstituteData]
        public void ToStream_Should_Have_StartPosition_Zero_InStream(
            Record typedObject)
            => sut.ToStream(typedObject)
                .Position
                .Should()
                .Be(0);

        [Theory, AutoNSubstituteData]
        public void ToStream_Should_Have_Content(
            Record typedObject)
            => sut.ToStream(typedObject)
                .Length
                .Should()
                .BeGreaterThan(0);

        [Theory, AutoNSubstituteData]
        public void FromStream_Should_Return_TypedObject(
           Record typedObject)
        {
            using var stream = sut.ToStream(typedObject);

            sut.FromStream<Record>(stream)
                .Should()
                .BeEquivalentTo(typedObject);
        }

        [Fact]
        public void FromStream_ShouldThrow_If_Stream_IsNull()
            => Invoking(() => sut.FromStream<Record>(null))
                .Should()
                .Throw<ArgumentNullException>();

        [Fact]
        public void FromStream_Should_Return_Null_If_Stream_IsEmpty()
            => sut.FromStream<Record>(Stream.Null)
                .Should()
                .BeNull();
    }
}