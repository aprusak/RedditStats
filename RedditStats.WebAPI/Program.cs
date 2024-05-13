using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedditStats.AppCore.Configuration;
using RedditStats.AppCore.Entities;
using RedditStats.AppCore.Services;
using RedditStats.Infra.Monitors;
using RedditStats.Infra.Queries;
using RedditStats.Infra.Reddit;
using RedditStats.Infra.Storage;

var builder = WebApplication.CreateBuilder(args);

// Read and bind config section
var section = builder.Configuration.GetRequiredSection("Config");
var config = section.Get<Config>() ?? throw new InvalidOperationException($"Can't bind configuration instance to the following instance type: `{nameof(Config)}`");

builder.Services.AddLogging();

builder.Services.AddEndpointsApiExplorer();

// Register in-memory-db
builder.Services.AddDbContext<InMemoryDb>(options => options.UseInMemoryDatabase("in-memory-db"), ServiceLifetime.Scoped);

// Register queries. To extend support for new stats a new queries may need to be added.
builder.Services.AddScoped(sp => new QueryErrorDecorator<RedditPost>(new RankedPostsQuery(sp.CreateAsyncScope().ServiceProvider.GetRequiredService<InMemoryDb>()), sp.GetRequiredService<ILogger<RankedPostsQuery>>()));
builder.Services.AddScoped(sp => new QueryErrorDecorator<RedditUser>(new RankedUsersQuery(sp.CreateAsyncScope().ServiceProvider.GetRequiredService<InMemoryDb>()), sp.GetRequiredService<ILogger<RankedUsersQuery>>()));
builder.Services.AddScoped(sp => new QueryErrorDecorator<RedditComment>(new RankedCommentsQuery(sp.CreateAsyncScope().ServiceProvider.GetRequiredService<InMemoryDb>()), sp.GetRequiredService<ILogger<RankedCommentsQuery>>()));

// Register Subreddit Factory
builder.Services.AddSingleton<ISubredditFactory, SubredditFactory>(sp => new SubredditFactory(config.AppId, config.AccessToken, config.RefreshToken, sp.GetRequiredService<ILogger<SubredditFactory>>()));

// Register monitors. To extend support for new stats a new monitors may need to be added.
builder.Services.AddTransient(sp => new PostsMonitor(sp.GetRequiredService<ISubredditFactory>().Create(config.Subreddit), sp.CreateAsyncScope().ServiceProvider.GetRequiredService<InMemoryDb>(), sp.GetRequiredService<ILogger<PostsMonitor>>()));
builder.Services.AddTransient(sp => new CommentsMonitor(sp.GetRequiredService<ISubredditFactory>().Create(config.Subreddit), sp.CreateAsyncScope().ServiceProvider.GetRequiredService<InMemoryDb>(), sp.GetRequiredService<ILogger<CommentsMonitor>>()));

var app = builder.Build();

// Start monitors
using var postsMonitor = app.Services.GetRequiredService<PostsMonitor>();
postsMonitor.Start(config.MonitoringInterval);

using var commentsMonitor = app.Services.GetRequiredService<CommentsMonitor>();
commentsMonitor.Start(config.MonitoringInterval);

// Using minimal API pattern
// Adding API methods for supported stats. To extend support for new stats new API methods may need to be added.
app.MapGet("/getrankedposts", async ([FromServices] QueryErrorDecorator<RedditPost> query) => TypedResults.Ok(await query.GetAsync().ConfigureAwait(false)));

app.MapGet("/getrankedusers", async ([FromServices] QueryErrorDecorator<RedditUser> query) => TypedResults.Ok(await query.GetAsync().ConfigureAwait(false)));

app.MapGet("/getrankedcomments", async ([FromServices] QueryErrorDecorator<RedditComment> query) => TypedResults.Ok(await query.GetAsync().ConfigureAwait(false)));

app.MapGet("/", () => $"RedditStats.WebAPI has started!");

app.Run();

// Stop monitors
postsMonitor.Stop();
commentsMonitor.Stop();