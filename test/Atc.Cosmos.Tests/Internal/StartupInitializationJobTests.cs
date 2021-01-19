using System.Threading;
using System.Threading.Tasks;
using Atc.Cosmos.Internal;
using Atc.Test;
using AutoFixture.AutoNSubstitute;
using AutoFixture.Xunit2;
using NSubstitute;
using Xunit;

namespace Atc.Cosmos.Tests.Internal
{
    public class StartupInitializationJobTests
    {
        [Theory, AutoNSubstituteData]
        public async Task Should_Initialize_Cosmos_OnStart(
            [Frozen, Substitute] ICosmosInitializer initializer,
            StartupInitializationJob sut,
            CancellationToken cancellationToken)
        {
            await sut.StartAsync(cancellationToken);

            await initializer
                .Received(1)
                .InitializeAsync(
                    Arg.Any<CancellationToken>());
        }
    }
}