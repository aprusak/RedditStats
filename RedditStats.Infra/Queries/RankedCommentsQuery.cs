using Microsoft.EntityFrameworkCore;
using RedditStats.AppCore.Entities;
using RedditStats.Infra.Storage;

namespace RedditStats.Infra.Queries
{
    public sealed class RankedCommentsQuery : QueryBase<RedditComment>
    {
        public RankedCommentsQuery(InMemoryDb db)
            : base(db)
        { }

        /// <summary>
        /// Queries Reddit comments from `InMemoryDb` ordering them by `UpVotes` desc.
        /// </summary>
        public override Task<List<RedditComment>> GetAsync(CancellationToken token = default)
        {
            return Db.Set<RedditComment>()
                .Select(c => new RedditComment { Id = c.Id, UpVotes = c.UpVotes, NumReplies = c.NumReplies, Author = c.Author, Permalink = $"https://reddit.com{c.Permalink}" })
                .OrderByDescending(c => c.UpVotes)
                .ToListAsync(token);
        }
    }
}
