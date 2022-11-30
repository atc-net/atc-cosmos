using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Azure.Cosmos;

namespace Atc.Cosmos.Internal
{
    /// <summary>
    /// Represents the registry of all registered <see cref="ICosmosContainerNameProvider"/> and associated <see cref="CosmosOptions"/>.
    /// </summary>
    public interface ICosmosContainerRegistry
    {
        /// <summary>
        /// Gets the default <see cref="CosmosOptions"/>.
        /// </summary>
        CosmosOptions DefaultOptions { get; }

        /// <summary>
        /// Gets the list of all registered <see cref="CosmosOptions"/>, including the default options.
        /// </summary>
        IReadOnlyList<CosmosOptions> Options { get; }

        /// <summary>
        /// Gets the <see cref="ICosmosContainerNameProvider"/> registered for the given type.
        /// If there are no provider registered for the requested type then a <see cref="NotSupportedException"/> is thrown.
        /// </summary>
        /// <typeparam name="TType">The type to lookup in the registry.</typeparam>
        /// <returns>The <see cref="ICosmosContainerNameProvider"/> found.</returns>
        ICosmosContainerNameProvider GetContainerForType<TType>();

        /// <summary>
        /// Gets the <see cref="ICosmosContainerNameProvider"/> registered for the given type.
        /// If there are no provider registered for the requested type then a <see cref="NotSupportedException"/> is thrown.
        /// </summary>
        /// <param name="resourceType">The type to lookup in the registry.</param>
        /// <returns>The <see cref="ICosmosContainerNameProvider"/> found.</returns>
        ICosmosContainerNameProvider GetContainerForType(Type resourceType);
    }
}