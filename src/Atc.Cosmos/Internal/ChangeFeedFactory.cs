using System;
using Microsoft.Azure.Cosmos;

namespace Atc.Cosmos.Internal
{
    public class ChangeFeedFactory : IChangeFeedFactory
    {
        private readonly ICosmosContainerProvider containerProvider;

        public ChangeFeedFactory(
            ICosmosContainerProvider containerProvider)
        {
            this.containerProvider = containerProvider;
        }

        public ChangeFeedProcessor Create<T>(
            Container.ChangesHandler<T> onChanges,
            Container.ChangeFeedMonitorErrorDelegate? onError = null,
            string? processorName = null)
        {
            var container = containerProvider.GetContainer<T>();

            var builder = container
                .GetChangeFeedProcessorBuilder(
                    processorName ?? container.Id,
                    onChanges)
                .WithInstanceName(Guid.NewGuid().ToString())
                .WithLeaseContainer(
                    containerProvider.GetContainer(LeasesContainerInitializer.ContainerId, containerProvider.GetDatabaseFor<T>()))
                .WithMaxItems(100)
                .WithPollInterval(TimeSpan.FromMilliseconds(1000))
                .WithStartTime(DateTime.MinValue.ToUniversalTime()); // Will start from the beginning of feed when no lease is found.

            if (onError != null)
            {
                builder = builder.WithErrorNotification(onError);
            }

            return builder.Build();
        }
    }
}