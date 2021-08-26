using System.Collections.Generic;

namespace Atc.Cosmos
{
    public class PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; set; }
            = System.Array.Empty<T>();

        public string? ContinuationToken { get; set; }
    }
}