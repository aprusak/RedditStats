using Microsoft.Extensions.Logging;
using Reddit.Controllers;
using Reddit.Controllers.EventArgs;
using RedditStats.AppCore.Entities;
using RedditStats.Infra.Storage;

namespace RedditStats.Infra.Monitors
{
    /// <summary>
    /// Monitors new user comments in a given Subreddit.
    /// </summary>
    public sealed class CommentsMonitor : MonitorBase
    {
        public CommentsMonitor(Subreddit subreddit, IRepository repository, ILogger<CommentsMonitor> logger)
            : base(subreddit, repository, logger)
        { }

        public override void Start(TimeSpan monitoringInterval)
        {
            Subreddit.Comments.MonitorNew((int)monitoringInterval.TotalMilliseconds); // Turn on monitoring
            Subreddit.Comments.NewUpdated += OnUpdate;
        }

        public override void Stop()
        {
            Subreddit.Comments.MonitorNew();  // Turn off the monitoring
            Subreddit.Comments.NewUpdated -= OnUpdate; // Subscribe to Subreddit new comment events
        }

        /// <summary>
        /// Adds new comments to `Repository`.
        /// Adds comments authors to `Repository`.
        /// Updates user `RedditUsers.TotalComments`.
        /// </summary>
        private async void OnUpdate(object? sender, CommentsUpdateEventArgs eventArgs)
        {
            foreach (var comment in eventArgs.Added)
            {
                // Add each new comment to `Repository`
                var entity = new RedditComment { Id = comment.Id, Created = comment.Created, Author = comment.Author, UpVotes = comment.UpVotes, Score = comment.Score, NumReplies = comment.NumReplies, Permalink = comment.Permalink };
                
                await Repository.InsertAsync(entity).ConfigureAwait(false);

                await Repository.SaveAsync().ConfigureAwait(false);

                var existingUser = await Repository.GetAsync<RedditUser>(u => u.Name == comment.Author).ConfigureAwait(false);

                if (existingUser != null)
                    // Update existing user total comments (in the monitoring window).
                    existingUser.TotalComments++;
                else
                    // Adding a new user to the `Repository`
                    await Repository.InsertAsync(new RedditUser { Name = comment.Author, TotalComments = 1 }).ConfigureAwait(false);

                // Save `Repository` changes
                await Repository.SaveAsync().ConfigureAwait(false);
            }
        }
    }
}
