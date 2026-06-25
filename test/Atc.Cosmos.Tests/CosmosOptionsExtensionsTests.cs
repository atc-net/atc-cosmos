namespace Atc.Cosmos.Tests;

public sealed class CosmosOptionsExtensionsTests
{
    [Theory, AutoNSubstituteData]
    public void UseCosmosEmulator_Sets_AccountEndpoint(CosmosOptions options)
    {
        // Act
        options.UseCosmosEmulator();

        // Assert
        options.AccountEndpoint.Should().Be("https://localhost:8081");
    }

    [Theory, AutoNSubstituteData]
    public void UseCosmosEmulator_Sets_AccountKey(CosmosOptions options)
    {
        // Act
        options.UseCosmosEmulator();

        // Assert
        options.AccountKey.Should().Be("C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==");
    }
}