using System;
using Atc.Cosmos.Testing;
using Atc.Test;
using FluentAssertions;
using Xunit;

namespace Atc.Cosmos.Tests.Testing
{
    public class CosmosResourceExtensionsTests
    {
        [Theory, AutoNSubstituteData]
        public void Should_Create_Clone(
            Record record)
        {
            record
                .Clone()
                .Should()
                .NotBeSameAs(record)
                .And
                .BeEquivalentTo(record);
        }

        [Theory, AutoNSubstituteData]
        public void Should_Create_Clone_Of_Nullable(
            Record? record)
        {
            record
                .Clone()
                .Should()
                .NotBeSameAs(record)
                .And
                .BeEquivalentTo(record);
        }
    }
}
