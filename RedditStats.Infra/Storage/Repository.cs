using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace RedditStats.Infra.Storage
{
    public sealed class Repository : IRepository, IDisposable
    {
        private DbContext _context;

        public Repository(DbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<T>> GetAsync<T>(CancellationToken token = default) where T : class
        {
            return await _context.Set<T>().ToArrayAsync(token).ConfigureAwait(false);
        }

        public async Task<T?> GetAsync<T>(Expression<Func<T, bool>> predicate, CancellationToken token = default) where T : class
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            return await _context.Set<T>().FirstOrDefaultAsync(predicate, token).ConfigureAwait(false);
        }

        public async Task InsertAsync<T>(T entity, CancellationToken token = default) where T : class
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            await _context.Set<T>().AddAsync(entity, token).ConfigureAwait(false);
        }

        public Task<int> SaveAsync(CancellationToken token = default)
        {
            return _context.SaveChangesAsync(token);
        }

        public Task UpdateAsync<T>(T entity, CancellationToken token = default) where T : class
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            _context.Entry(entity).State = EntityState.Modified;

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
