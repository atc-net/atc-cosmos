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
            [Frozen] string name)
            => new CosmosContainerNameProvider<Record>(name, null)
                .ContainerName
                .Should()
                .Be(name);

        [Theory, AutoNSubstituteData]
        public void Returns_Correct_Options_For_Type(
            [Frozen] string name,
            CosmosOptions options)
            => new CosmosContainerNameProvider<Record>(name, options)
                .Options
                .Should()
                .Be(options);

        [Fact]
        public void Returns_Correct_Match_For_Type()
            => new CosmosContainerNameProvider<Record>("name", null)
                .IsForType(typeof(Record))
                .Should()
                .BeTrue();

        [Fact]
        public void Returns_False_For_Incorrect_Type()
            => new CosmosContainerNameProvider<Record>("name", null)
                .IsForType(typeof(string))
                .Should()
                .BeFalse();

        [Theory, AutoNSubstituteData]
        public void Returns_Correct_Match_For_Generic_Type(
            string name,
            CosmosOptions options)
            => new CosmosContainerNameProvider(typeof(Record<>), name, options)
                .IsForType(typeof(Record<string>))
                .Should()
                .BeTrue();

        [Theory, AutoNSubstituteData]
        public void Returns_Null_For_Incorrect_Generic_Type(
            string name,
            CosmosOptions options)
            => new CosmosContainerNameProvider(typeof(Record<Record>), name, options)
                .IsForType(typeof(Record<string>))
                .Should()
                .BeFalse();
    }
}