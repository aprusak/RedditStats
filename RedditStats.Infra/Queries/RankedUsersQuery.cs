using Microsoft.EntityFrameworkCore;
using RedditStats.AppCore.Entities;
using RedditStats.Infra.Queries;
using RedditStats.Infra.Storage;

namespace RedditStats.AppCore.Services
{
    public sealed class RankedUsersQuery : QueryBase<RedditUser>
    {
        public RankedUsersQuery(InMemoryDb db)
            : base(db)
        { }

        /// <summary>
        /// Queries Reddit users from `InMemoryDb` ordering them by `TotalPosts + TotalComments` desc.
        /// </summary>
        public override Task<List<RedditUser>> GetAsync(CancellationToken token = default)
        {
            return Db.RedditUsers
                .Select(u => new RedditUser {Id = u.Id, Name = u.Name, TotalPosts = u.TotalPosts, TotalComments = u.TotalComments })
                .OrderByDescending(u => u.TotalPosts + u.TotalComments) // Order by `u.TotalPosts + u.TotalComments` desc
                .ToListAsync(token);
        }
    }
}
