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
        public CommentsMonitor(Subreddit subreddit, InMemoryDb db, ILogger<CommentsMonitor> logger)
            : base(subreddit, db, logger)
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
        /// Adds new comments to the `InMemoryDb.RedditComments`.
        /// Adds comment authors to the `InMemoryDb.RedditUsers`.
        /// Updates user `RedditUsers.TotalComments`.
        /// </summary>
        private void OnUpdate(object? sender, CommentsUpdateEventArgs eventArgs)
        {
            foreach (var comment in eventArgs.Added)
            {
                // Add each post to `db`
                Db.Add(new RedditComment { Id = comment.Id, Created = comment.Created, Author = comment.Author, UpVotes = comment.UpVotes, Score = comment.Score, NumReplies = comment.NumReplies, Permalink = comment.Permalink });

                Db.SaveChanges();

                var existingUser = Db.RedditUsers.FirstOrDefault(u => u.Name == comment.Author);

                if (existingUser != null)
                    // If the user from the comments exists in the `db` (in monitoring window) increment the user total comments.
                    existingUser.TotalComments++;

                else
                    // Adding a new user to the `db`
                    Db.Add(new RedditUser { Name = comment.Author, TotalComments = 1 });

                // Save `db` changes
                Db.SaveChanges();
            }
        }
    }
}
