using Microsoft.Extensions.Logging;
using Reddit.Controllers;
using RedditStats.Infra.Storage;

namespace RedditStats.Infra.Monitors
{
    /// <summary>
    /// Monitor base class.
    /// </summary>
    public abstract class MonitorBase : IDisposable
    {
        protected MonitorBase(Subreddit subreddit, InMemoryDb db, ILogger logger)
        {
            Subreddit = subreddit ?? throw new ArgumentNullException(nameof(subreddit));
            Db = db ?? throw new ArgumentNullException(nameof(db));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected ILogger Logger { get; }
        protected Subreddit Subreddit { get; }
        protected InMemoryDb Db { get; }

        public abstract void Start(TimeSpan monitoringInterval);

        public abstract void Stop();

        public void Dispose()
        {
            Stop();
        }
    }
}
