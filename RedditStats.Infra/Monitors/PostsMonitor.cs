using Microsoft.Extensions.Logging;
using Reddit.Controllers;
using Reddit.Controllers.EventArgs;
using RedditStats.AppCore.Entities;
using RedditStats.Infra.Storage;

namespace RedditStats.Infra.Monitors
{
    /// <summary>
    /// Monitors new user posts in a given Subreddit.
    /// </summary>
    public sealed class PostsMonitor : MonitorBase
    {
        public PostsMonitor(Subreddit subreddit, IRepository repository, ILogger<PostsMonitor> logger)
            : base(subreddit, repository, logger)
        { }
        
        public override void Start(TimeSpan monitoringInterval)
        {
            Subreddit.Posts.MonitorNew((int)monitoringInterval.TotalMilliseconds); // Turn on monitoring
            Subreddit.Posts.NewUpdated += OnUpdate;
        }

        public override void Stop()
        {
            Subreddit.Posts.MonitorNew();  // Turn off the monitoring
            Subreddit.Posts.NewUpdated -= OnUpdate; // Subscribe to Subreddit new post events
        }

        /// <summary>
        /// Adds new posts to `Repository`.
        /// Adds post authors to `Repository`.
        /// Updates user `RedditUsers.TotalPosts`.
        /// </summary>
        private async void OnUpdate(object? sender, PostsUpdateEventArgs eventArgs)
        {
            foreach (Post post in eventArgs.Added)
            {
                var entity = new RedditPost { PostId = post.Id, Created = post.Created, Author = post.Author, Title = post.Title, UpVotes = post.UpVotes, Score = post.Score, Permalink = post.Permalink };

                // Add each new post to `Repository`
                await Repository.InsertAsync(entity).ConfigureAwait(false);

                await Repository.SaveAsync().ConfigureAwait(false);

                var existingUser = await Repository.GetAsync<RedditUser>(u => u.Name == post.Author).ConfigureAwait(false);

                if (existingUser != null)
                    // Update existing user total posts (in the monitoring window).
                    existingUser.TotalPosts++;
                else
                    // Adding a new user to the `Repository`
                    await Repository.InsertAsync(new RedditUser { Name = post.Author, TotalPosts = 1 }).ConfigureAwait(false);

                // Save `Repository` changes
                await Repository.SaveAsync().ConfigureAwait(false);
            }
        }
    }
}
