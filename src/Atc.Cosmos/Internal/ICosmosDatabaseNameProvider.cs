using System.Collections.Generic;

namespace Atc.Cosmos.Internal
{
    /// <summary>
    /// Represents access to all database names registered with containers.
    /// </summary>
    public interface ICosmosDatabaseNameProvider
    {
        /// <summary>
        /// Gets the list of available databases registered.
        /// </summary>
        IReadOnlyList<string> DatabaseNames { get; }

        /// <summary>
        /// Gets the default database name which corresponds to the value from <see cref="CosmosOptions"/> instance.
        /// </summary>
        string DefaultDatabaseName { get; }

        /// <summary>
        /// Gets the database name from the given <see cref="ICosmosContainerNameProvider"/> and if the name is absent then the default database name is returned.
        /// </summary>
        /// <param name="provider">The provider to extract the database name from.</param>
        /// <returns>The database name from the provider or if it's absent then the default database is returned.</returns>
        string ResolveDatabaseName(ICosmosContainerNameProvider provider);
    }
}