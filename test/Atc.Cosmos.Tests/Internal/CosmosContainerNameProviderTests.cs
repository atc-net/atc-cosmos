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
                .ContainerName
                .Should()
                .Be(name);

        [Theory, AutoNSubstituteData]
        public void Returns_Correct_Match_For_Type(
            CosmosContainerNameProvider<Record> sut)
            => sut
                .IsForType(typeof(Record))
                .Should()
                .BeTrue();

        [Theory, AutoNSubstituteData]
        public void Returns_False_For_Incorrect_Type(
            CosmosContainerNameProvider<Record> sut)
            => sut
                .IsForType(typeof(string))
                .Should()
                .BeFalse();

        [Theory, AutoNSubstituteData]
        public void Returns_Correct_Match_For_Generic_Type(
            string name,
            string database)
            => new CosmosContainerNameProvider(typeof(Record<>), name, database)
                .IsForType(typeof(Record<string>))
                .Should()
                .BeTrue();

        [Theory, AutoNSubstituteData]
        public void Returns_Null_For_Incorrect_Generic_Type(
            string name,
            string database)
            => new CosmosContainerNameProvider(typeof(Record<Record>), name, database)
                .IsForType(typeof(Record<string>))
                .Should()
                .BeFalse();
    }
}