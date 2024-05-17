using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RedditStats.AppCore.Entities;

namespace RedditStats.Infra.Storage
{
    /// <summary>
    /// In memory DB.
    /// </summary>
    public sealed class InMemoryDb : DbContext
    {
        readonly ILogger<InMemoryDb> _logger;

        public InMemoryDb(DbContextOptions<InMemoryDb> options, ILogger<InMemoryDb> logger)
            : base(options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Register defined entities
            modelBuilder.Entity<RedditPost>();
            modelBuilder.Entity<RedditUser>();
            modelBuilder.Entity<RedditComment>();
        }

        public override int SaveChanges(bool acceptAllchangesOnSuccess = true)
        {
            try
            {
                return base.SaveChanges(acceptAllchangesOnSuccess);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"`{nameof(SaveChanges)}` has failed.");
            }

            return 0;
        }
    }
}
