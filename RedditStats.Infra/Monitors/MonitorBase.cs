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
        protected MonitorBase(Subreddit subreddit, IRepository repository, ILogger logger)
        {
            Subreddit = subreddit ?? throw new ArgumentNullException(nameof(subreddit));
            Repository = repository ?? throw new ArgumentNullException(nameof(repository));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected ILogger Logger { get; }
        protected Subreddit Subreddit { get; }
        protected IRepository Repository { get; }

        public abstract void Start(TimeSpan monitoringInterval);

        public abstract void Stop();

        public void Dispose()
        {
            Stop();
        }
    }
}
