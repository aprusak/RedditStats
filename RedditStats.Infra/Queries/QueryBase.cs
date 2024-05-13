using RedditStats.Infra.Storage;

namespace RedditStats.Infra.Queries
{
    /// <summary>
    /// Query base class.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class QueryBase<T> : IQueryAsync<T>
    {
        protected QueryBase(InMemoryDb db)
        {
            Db = db ?? throw new ArgumentNullException(nameof(db));
        }

        protected InMemoryDb Db { get; }

        public abstract Task<List<T>> GetAsync(CancellationToken token = default);
    }
}
