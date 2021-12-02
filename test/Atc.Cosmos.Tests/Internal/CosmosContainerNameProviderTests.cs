using Atc.Cosmos.Internal;
using Atc.Test;
using AutoFixture.Xunit2;
using FluentAssertions;
using Xunit;

namespace Atc.Cosmos.Tests.Internal
{
    public class CosmosContainerNameProviderTests
    {
        [Theory, AutoNSubstituteData]
        public void Returns_Correct_ContainerName_For_Type(
            [Frozen] string name,
            CosmosContainerNameProvider<Record> sut)
            => sut
                .GetContainerName(typeof(Record))
                .Should()
                .Be(name);

        [Theory, AutoNSubstituteData]
        public void Returns_Null_For_Incorrect_Type(
            [Frozen] string name,
            CosmosContainerNameProvider<Record> sut)
            => sut
                .GetContainerName(typeof(string))
                .Should()
                .NotBe(name)
                .And
                .BeNull();

        [Theory, AutoNSubstituteData]
        public void Returns_Correct_ContainerName_For_Generic_Type(
            string name)
            => new CosmosContainerNameProvider(typeof(Record<>), name)
                .GetContainerName(typeof(Record<string>))
                .Should()
                .Be(name);
    }
}