using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;

namespace Atc.Cosmos.Tests.Generators
{
    internal sealed class FakeTokenCredential : TokenCredential
    {
        private readonly AccessToken accessToken = new (
            "token",
            DateTimeOffset.UtcNow.AddDays(10));

        public override AccessToken GetToken(
            TokenRequestContext requestContext,
            CancellationToken cancellationToken)
            => accessToken;

        public override ValueTask<AccessToken> GetTokenAsync(
            TokenRequestContext requestContext,
            CancellationToken cancellationToken)
            => ValueTask.FromResult(accessToken);
    }
}
