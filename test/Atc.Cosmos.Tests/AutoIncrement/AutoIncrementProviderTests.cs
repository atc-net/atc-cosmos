using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Atc.Cosmos.AutoIncrement;
using Atc.Test;
using AutoFixture;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using Xunit;

namespace Atc.Cosmos.Tests.AutoIncrement
{
    public class AutoIncrementProviderTests
    {
        private readonly ICosmosWriter<AutoIncrementCounter> writer;
        private readonly AutoIncrementProvider sut;
        private readonly AutoIncrementCounter counter;

        [SuppressMessage("Minor Code Smell", "S1905:Redundant casts should not be used", Justification = "Require By NSubstitute")]
        public AutoIncrementProviderTests()
        {
            counter = new Fixture().Create<AutoIncrementCounter>();
            writer = Substitute.For<ICosmosWriter<AutoIncrementCounter>>();
            writer
                .UpdateOrCreateAsync(default!, (Action<AutoIncrementCounter>)default!, default, default)
                .ReturnsForAnyArgs(counter);

            sut = new AutoIncrementProvider(writer);
        }

        [Theory, AutoNSubstituteData]
        public async Task GetNextAsync_Calls_UpdateOrCreateAsync(
            string counterName,
            CancellationToken cancellationToken)
        {
            await sut.GetNextAsync(counterName, cancellationToken);

            _ = writer
                .Received(1)
                .UpdateOrCreateAsync(
                    Arg.Any<Func<AutoIncrementCounter>>(),
                    Arg.Any<Action<AutoIncrementCounter>>(),
                    retries: 5,
                    cancellationToken);
        }

        [Theory, AutoNSubstituteData]
        public async Task GetNextAsync_Calls_UpdateOrCreateAsync_With_Correct_Factory(
            string counterName,
            CancellationToken cancellationToken)
        {
            await sut.GetNextAsync(counterName, cancellationToken);

            writer
                .ReceivedCallWithArgument<Func<AutoIncrementCounter>>()
                .Invoke()
                .Should()
                .BeEquivalentTo(new AutoIncrementCounter
                {
                    CounterName = counterName,
                });
        }

        [Theory, AutoNSubstituteData]
        public async Task GetNextAsync_Calls_UpdateOrCreateAsync_With_Correct_Updater(
            string counterName,
            AutoIncrementCounter counter,
            CancellationToken cancellationToken)
        {
            await sut.GetNextAsync(counterName, cancellationToken);

            var expectedCcunt = counter.Count + 1;
            writer
                .ReceivedCallWithArgument<Action<AutoIncrementCounter>>()
                .Invoke(counter);

            counter.Count
                .Should()
                .Be(expectedCcunt);
        }

        [Theory, AutoNSubstituteData]
        public async Task GetNextAsync_Returns_Updated_Count(
            string counterName,
            int updatedCount,
            CancellationToken cancellationToken)
        {
            counter.Count = updatedCount;

            var result = await sut.GetNextAsync(counterName, cancellationToken);
            result
                .Should()
                .Be(updatedCount);
        }
    }
}