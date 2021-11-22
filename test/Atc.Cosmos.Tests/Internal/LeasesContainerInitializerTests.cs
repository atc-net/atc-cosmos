using System.Threading;
using System.Threading.Tasks;
using Atc.Cosmos.Internal;
using Atc.Test;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using NSubstitute;
using Xunit;

namespace Atc.Cosmos.Tests.Internal
{
    public class LeasesContainerInitializerTests
    {
        [Theory, AutoNSubstituteData]
        public async Task Should_Create_Cosmos_Container(
            LeasesContainerInitializer sut,
            Database database,
            CancellationToken cancellationToken)
        {
            await sut.InitializeAsync(database, cancellationToken);

            _ = database
                .Received(1)
                .CreateContainerIfNotExistsAsync(
                    Arg.Any<ContainerProperties>(),
                    throughput: null,
                    requestOptions: null,
                    cancellationToken: cancellationToken);

            var options = database
                .ReceivedCallWithArgument<ContainerProperties>();

            options.IndexingPolicy.Automatic
                .Should()
                .BeTrue();
            options.IndexingPolicy.IndexingMode
                .Should()
                .Be(IndexingMode.Consistent);
            options.IndexingPolicy.ExcludedPaths
                .Should()
                .ContainEquivalentOf(new ExcludedPath { Path = "/*" });

            options.Id
                .Should()
                .Be("leases");
            options.PartitionKeyPath
                .Should()
                .Be("/id");
        }
    }
}