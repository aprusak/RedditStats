using Microsoft.Extensions.Logging;

namespace RedditStats.Infra.Queries
{
    /// <summary>
    /// Query decorator that adds exception logging (handling can be added too)
    /// for the decorated instance (`nested`).
    /// </summary>
    /// <typeparam name="T">Type fo the items returned by the query.</typeparam>
    public sealed class QueryErrorDecorator<T> : IQueryAsync<T>
    {
        readonly IQueryAsync<T> _nested;
        readonly ILogger<IQueryAsync<T>> _logger;

        public QueryErrorDecorator(IQueryAsync<T> nested, ILogger<IQueryAsync<T>> logger)
        {
            _nested = nested ?? throw new ArgumentNullException(nameof(nested));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<T>> GetAsync(CancellationToken token = default)
        {
            try
            {
                return await _nested.GetAsync(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"`{_nested.GetType().Name}:{nameof(GetAsync)}` has failed.");
            }

            return new List<T>();
        }
    }
}
