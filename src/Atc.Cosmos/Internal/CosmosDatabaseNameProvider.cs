using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;

namespace Atc.Cosmos.Internal
{

    public class CosmosDatabaseNameProvider : ICosmosDatabaseNameProvider
    {
        public CosmosDatabaseNameProvider(IEnumerable<ICosmosContainerNameProvider> providers, OptionsWrapper<CosmosOptions> options)
        {
            var names = providers.Where(p => p.DatabaseName != null).Select(p => p.DatabaseName).Distinct().ToList();

            // make sure default database exists in the list
            if (!names.Contains(options.Value.DatabaseName))
            {
                names.Add(options.Value.DatabaseName);
            }

#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
            DatabaseNames = names; // null values has been removed in the linq expression above.
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
            DefaultDatabaseName = options.Value.DatabaseName;
        }

        public IReadOnlyList<string> DatabaseNames { get; }

        public string DefaultDatabaseName { get; }

        public string ResolveDatabaseName(ICosmosContainerNameProvider provider)
            => provider.DatabaseName ?? DefaultDatabaseName;
    }
}