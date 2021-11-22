using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Atc.Cosmos.Internal;
using Atc.Test;
using AutoFixture;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using Xunit;

namespace Atc.Cosmos.Tests.Internal
{
    public class ChangeFeedServiceTests
    {
        private readonly IChangeFeedListener[] listeners;
        private readonly ChangeFeedService sut;

        public ChangeFeedServiceTests()
        {
            listeners = FixtureFactory.Create().Create<IChangeFeedListener[]>();

            sut = new ChangeFeedService(
                listeners);
        }

        [Fact]
        public void Should_Implement_IHostedService()
            => sut
                .Should()
                .BeAssignableTo<IHostedService>();

        [Theory, AutoNSubstituteData]
        public async Task StartAsync_Should_Call_Start_On_Listeners(
            CancellationToken cancellationToken)
        {
            await sut.StartAsync(cancellationToken);

            foreach (var listener in listeners)
            {
                _ = listener
                    .Received(1)
                    .StartAsync(cancellationToken);
            }
        }

        [Theory, AutoNSubstituteData]
        public async Task StopAsync_Should_Call_Stop_On_Listeners(
            CancellationToken cancellationToken)
        {
            await sut.StopAsync(cancellationToken);

            foreach (var listener in listeners)
            {
                _ = listener
                    .Received(1)
                    .StopAsync(cancellationToken);
            }
        }
    }
}