using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Atc.Cosmos.AutoIncrement;
using Atc.Test;
using AutoFixture.AutoNSubstitute;
using Microsoft.Azure.Cosmos;
using NSubstitute;
using Xunit;

namespace Atc.Cosmos.Tests.AutoIncrement
{
    public class AutoIncrementCounterInitializerTests
    {
        [Theory, AutoNSubstituteData]
        public async Task Should_Initialize_Container_With_Id_As_PartitionKey(
            [Substitute] Database database,
            AutoIncrementCounterInitializer sut,
            CancellationToken cancellationToken)
        {
            await sut.InitializeAsync(database, cancellationToken);

            await database
                .Received(1)
                .CreateContainerIfNotExistsAsync(
                    Arg.Is<ContainerProperties>(p => p.PartitionKeyPath == "/id"),
                    Arg.Any<int?>(),
                    Arg.Any<RequestOptions>(),
                    Arg.Any<CancellationToken>());
        }

        [Theory, AutoNSubstituteData]
        public async Task Should_Exclude_All_From_Indexing(
            [Substitute] Database database,
            AutoIncrementCounterInitializer sut,
            CancellationToken cancellationToken)
        {
            await sut.InitializeAsync(database, cancellationToken);

            await database
                .Received(1)
                .CreateContainerIfNotExistsAsync(
                    Arg.Is<ContainerProperties>(p => p.IndexingPolicy.ExcludedPaths.Any(p => p.Path == "/*")),
                    Arg.Any<int?>(),
                    Arg.Any<RequestOptions>(),
                    Arg.Any<CancellationToken>());
        }

        [Theory, AutoNSubstituteData]
        public async Task Should_Enable_Automatic_Indexing(
            [Substitute] Database database,
            AutoIncrementCounterInitializer sut,
            CancellationToken cancellationToken)
        {
            await sut.InitializeAsync(database, cancellationToken);

            await database
                .Received(1)
                .CreateContainerIfNotExistsAsync(
                    Arg.Is<ContainerProperties>(p => p.IndexingPolicy.Automatic),
                    Arg.Any<int?>(),
                    Arg.Any<RequestOptions>(),
                    Arg.Any<CancellationToken>());
        }

        [Theory, AutoNSubstituteData]
        public async Task Should_Use_Consistant_IndexingMode(
            [Substitute] Database database,
            AutoIncrementCounterInitializer sut,
            CancellationToken cancellationToken)
        {
            await sut.InitializeAsync(database, cancellationToken);

            await database
                .Received(1)
                .CreateContainerIfNotExistsAsync(
                    Arg.Is<ContainerProperties>(p => p.IndexingPolicy.IndexingMode == IndexingMode.Consistent),
                    Arg.Any<int?>(),
                    Arg.Any<RequestOptions>(),
                    Arg.Any<CancellationToken>());
        }
    }
}