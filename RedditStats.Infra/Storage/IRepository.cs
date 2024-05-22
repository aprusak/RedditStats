using System.Linq.Expressions;

namespace RedditStats.Infra.Storage
{
    public interface IRepository
    {
        Task<IEnumerable<T>> GetAsync<T>(CancellationToken token = default) where T : class;

        Task InsertAsync<T>(T entity, CancellationToken token = default) where T : class;

        Task UpdateAsync<T>(T entity, CancellationToken token = default) where T : class;

        Task<T?> GetAsync<T>(Expression<Func<T, bool>> predicate, CancellationToken token = default) where T : class;

        Task<int> SaveAsync(CancellationToken token = default);
    }
}
