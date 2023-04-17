using System;

namespace Atc.Cosmos
{
    /// <summary>
    /// Options for configuring Cosmos change feed processor.
    /// </summary>
    public class ChangeFeedProcessorOptions
    {
        /// <summary>
        /// Gets or sets the delay in between polling the change feed for new changes, after all current changes are drained.
        /// Default value is 1000ms.
        /// <remarks>
        /// Applies only after a read on the change feed yielded no results.
        /// </remarks>
        /// </summary>
        public TimeSpan FeedPollDelay { get; set; } = TimeSpan.FromMilliseconds(1000);

        /// <summary>
        /// Gets or sets the maximum number of items to be returned in the enumeration operation in the Azure Cosmos DB service.
        /// Default value is 100.
        /// </summary>
        public int MaxItemCount { get; set; } = 100;

        /// The maximum parallel calls to the <see cref="IChangeFeedProcessor{T}"/>.
        /// Default value is 1.
        public int MaxDegreeOfParallelism { get; set; } = 1;

        public static ChangeFeedProcessorOptions Default()
        {
            return new ChangeFeedProcessorOptions();
        }
    }
}
