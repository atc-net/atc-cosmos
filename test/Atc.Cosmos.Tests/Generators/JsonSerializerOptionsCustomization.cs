namespace Atc.Cosmos.Tests.Generators;

[AutoRegister]
public class JsonSerializerOptionsCustomization : ICustomization
{
    // AutoFixture would otherwise assign random values to every writable
    // property on JsonSerializerOptions, including IndentCharacter (added in
    // .NET 9), which only accepts a space or horizontal tab and throws for any
    // other character. Hand back a default instance instead.
    public void Customize(IFixture fixture)
        => fixture.Register(() => new JsonSerializerOptions());
}