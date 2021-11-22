using System;
using System.Collections.Generic;
using System.Linq;
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
    public class ChangeFeedListenerTests
    {
        public class RecordProcessor : IChangeFeedProcessor<Record>
        {
            public virtual Task ProcessAsync(
                string partitionKey,
                IReadOnlyCollection<Record> changes,
                CancellationToken cancellationToken)
                => Task.CompletedTask;

            public Task ErrorAsync(
                string leaseToken,
                Exception exception)
                => Task.CompletedTask;
        }

        private readonly RecordProcessor processor;
        private readonly ChangeFeedProcessor changeFeed;
        private readonly IChangeFeedFactory factory;
        private readonly ChangeFeedListener<Record, RecordProcessor> sut;

        public ChangeFeedListenerTests()
        {
            processor = Substitute.For<RecordProcessor>();
            changeFeed = Substitute.For<ChangeFeedProcessor>();
            factory = Substitute.For<IChangeFeedFactory>();
            factory
                .Create<Record>(default)
                .ReturnsForAnyArgs(changeFeed);

            sut = new ChangeFeedListener<Record, RecordProcessor>(
                factory,
                processor);
        }

        [Fact]
        public void Should_Create_ChangeFeedProcessor_Using_Factory()
        {
            factory
                .Received(1)
                .Create<Record>(
                    Arg.Any<Container.ChangesHandler<Record>>(),
                    Arg.Any<Container.ChangeFeedMonitorErrorDelegate>());
        }

        [Theory, AutoNSubstituteData]
        public void Should_Call_Processor_With_Partitioned_Data_From_ChangeFeed(
            Record[][] records,
            string[] partitionKeys,
            CancellationToken cancellationToken)
        {
            for (var i = 0; i < partitionKeys.Length; i++)
            {
                foreach (var record in records[i])
                {
                    record.Pk = partitionKeys[i];
                }
            }

            var onChanges = factory
                .ReceivedCallWithArgument<
                    Container.ChangesHandler<Record>>();

            onChanges.Invoke(
                records
                    .SelectMany(a => a)
                    .OrderBy(a => Guid.NewGuid())
                    .ToArray(),
                cancellationToken);

            foreach (var pk in partitionKeys)
            {
                processor
                    .Received(1)
                    .ProcessAsync(
                        pk,
                        Arg.Any<IReadOnlyCollection<Record>>(),
                        cancellationToken);
            }

            processor
                .ReceivedCallsWithArguments<
                    IReadOnlyCollection<Record>>()
                .Should()
                .BeEquivalentTo(records);
        }

        [Theory, AutoNSubstituteData]
        public async Task StartAsync_Should_Start_ChangeFeedProcessor(
                CancellationToken cancellationToken)
        {
            await sut.StartAsync(cancellationToken);

            _ = changeFeed
                .Received(1)
                .StartAsync();
        }

        [Theory, AutoNSubstituteData]
        public async Task StopAsync_Should_Stop_ChangeFeedProcessor(
                CancellationToken cancellationToken)
        {
            await sut.StopAsync(cancellationToken);

            _ = changeFeed
                .Received(1)
                .StopAsync();
        }
    }
}