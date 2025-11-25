using Atc.Test.Customizations;
using AutoFixture;

namespace Atc.Cosmos.Tests;

/// <summary>
/// Customize CosmosOptions to allow AutoFixture to create it.
/// </summary>
/// <remarks>
/// This class is "magically" instantiated via Atc.Test.AutoNSubstituteDataAttribute
/// </remarks>
[AutoRegister]
public class CosmosOptionsCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        fixture.Customize<CosmosOptions>(composer =>
            composer.FromFactory(() => new CosmosOptions
                {
                    AccountEndpoint = "SomeEndpoint",
                    AccountKey = "SomeAccountKey",
                    DatabaseName = "SomeDatabase",
                })
                .OmitAutoProperties());
    }
}