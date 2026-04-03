namespace Atc.Cosmos.Tests.Generators;

[AutoRegister]
public class TokenCredentialGenerator : TypeRelay
{
    public TokenCredentialGenerator()
        : base(typeof(TokenCredential), typeof(FakeTokenCredential))
    {
    }
}