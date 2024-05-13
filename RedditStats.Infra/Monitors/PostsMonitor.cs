using Microsoft.Extensions.Logging;
using Reddit.Controllers;
using Reddit.Controllers.EventArgs;
using RedditStats.AppCore.Entities;
using RedditStats.Infra.Monitors;
using RedditStats.Infra.Storage;

namespace RedditStats.AppCore.Services
{
    /// <summary>
    /// Monitors new user posts in a given Subreddit.
    /// </summary>
    public sealed class PostsMonitor : MonitorBase
    {
        public PostsMonitor(Subreddit subreddit, InMemoryDb db, ILogger<PostsMonitor> logger)
            : base(subreddit, db, logger)
        { }
        
        public override void Start(TimeSpan monitoringInterval)
        {
            Subreddit.Posts.GetNew();
            Subreddit.Posts.MonitorNew((int)monitoringInterval.TotalMilliseconds); // Turn on monitoring
            Subreddit.Posts.NewUpdated += OnUpdate;
        }

        public override void Stop()
        {
            Subreddit.Posts.MonitorNew();  // Turn off the monitoring
            Subreddit.Posts.NewUpdated -= OnUpdate; // Subscribe to Subreddit new post events
        }

        /// <summary>
        /// Adds new posts to the `InMemoryDb.RedditPosts`.
        /// Adds post authors to the `InMemoryDb.RedditUsers`.
        /// Updates user `RedditUsers.TotalPosts`.
        /// </summary>
        private void OnUpdate(object? sender, PostsUpdateEventArgs eventArgs)
        {
            foreach (Post post in eventArgs.Added)
            {
                // Add each post to `db`
                Db.Add(new RedditPost { PostId = post.Id, Created = post.Created, Author = post.Author, Title = post.Title, UpVotes = post.UpVotes, Score = post.Score, Permalink = post.Permalink });

                Db.SaveChanges();

                var existingUser = Db.RedditUsers.FirstOrDefault(u => u.Name == post.Author);

                if (existingUser != null)
                    // If the user from the post exists in the `db` (in monitoring window) increment the user total posts.
                    existingUser.TotalPosts++;
                else
                    // Adding a new user to the `db`
                    Db.Add(new RedditUser { Name = post.Author, TotalPosts = 1 });

                // Save `db` changes
                Db.SaveChanges();
            }
        }
    }
}
