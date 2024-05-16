using Microsoft.EntityFrameworkCore;
using RedditStats.AppCore.Entities;
using RedditStats.Infra.Storage;

namespace RedditStats.Infra.Queries
{
    public sealed class RankedPostsQuery : QueryBase<RedditPost>
    {
        public RankedPostsQuery(InMemoryDb db)
            : base(db)
        { }

        /// <summary>
        /// Queries Reddit posts from `InMemoryDb` ordering them by `UpVotes` desc.
        /// </summary>
        public override Task<List<RedditPost>> GetAsync(CancellationToken token = default)
        {
            return Db.RedditPosts
                .Select(p => new RedditPost { Id = p.Id, PostId = p.PostId, UpVotes = p.UpVotes, Title = p.Title, Author = p.Author, Permalink = $"https://reddit.com{p.Permalink}" })
                .OrderByDescending(p => p.UpVotes)
                .ToListAsync(token);
        }
    }
}
