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
        public void Has_Correct_ContainerName(
            [Frozen] string name,
            CosmosContainerNameProvider<Record> sut)
            => sut.ContainerName
                .Should()
                .Be(name);

        [Theory, AutoNSubstituteData]
        public void Has_Correct_FromType(
            CosmosContainerNameProvider<Record> sut)
            => sut.FromType
                .Should()
                .Be(typeof(Record));
    }
}