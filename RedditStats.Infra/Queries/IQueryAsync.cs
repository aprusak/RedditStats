namespace RedditStats.Infra.Queries
{
    /// <summary>
    /// Async query contract.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IQueryAsync<T>
    {
        Task<List<T>> GetAsync(CancellationToken token = default);
    }
}
