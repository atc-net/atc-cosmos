namespace Atc.Cosmos;

public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; set; } = [];

    public string? ContinuationToken { get; set; }
}