using AutoFixture.Xunit2;
using FluentAssertions;
using Xunit;
using SUT = Atc.Cosmos.CosmosOptionsExtensions;

namespace Atc.Cosmos.Tests
{
    public class CosmosOptionsExtensionsTests
    {
        [Theory, AutoData]
        public void UseCosmosEmulator_Sets_AccountEndpoint(
            CosmosOptions options)
        {
            SUT.UseCosmosEmulator(options);

            options.AccountEndpoint
                .Should()
                .Be("https://localhost:8081");
        }

        [Theory, AutoData]
        public void UseCosmosEmulator_Sets_AccountKey(
            CosmosOptions options)
        {
            SUT.UseCosmosEmulator(options);

            options.AccountKey
                .Should()
                .Be("C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==");
        }
    }
}