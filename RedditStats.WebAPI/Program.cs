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

// Register in-memory-db
builder.Services.AddDbContext<InMemoryDb>(options => options.UseInMemoryDatabase("in-memory-db"), ServiceLifetime.Transient, ServiceLifetime.Transient);

// Register queries. To extend support for new stats new queries may need to be added.
builder.Services.AddSingleton(sp => new QueryErrorDecorator<RedditPost>(new RankedPostsQuery(sp.GetRequiredService<InMemoryDb>()), sp.GetRequiredService<ILogger<RankedPostsQuery>>()));
builder.Services.AddSingleton(sp => new QueryErrorDecorator<RedditUser>(new RankedUsersQuery(sp.GetRequiredService<InMemoryDb>()), sp.GetRequiredService<ILogger<RankedUsersQuery>>()));
builder.Services.AddSingleton(sp => new QueryErrorDecorator<RedditComment>(new RankedCommentsQuery(sp.GetRequiredService<InMemoryDb>()), sp.GetRequiredService<ILogger<RankedCommentsQuery>>()));

// Register Subreddit Factory
builder.Services.AddSingleton<ISubredditFactory, SubredditFactory>(sp => new SubredditFactory(config.AppId, config.AccessToken, config.RefreshToken, sp.GetRequiredService<ILogger<SubredditFactory>>()));

// Register monitors. To extend support for new stats new monitors may need to be added.
builder.Services.AddSingleton(sp => new PostsMonitor(sp.GetRequiredService<ISubredditFactory>().Create(config.Subreddit), sp.GetRequiredService<InMemoryDb>(), sp.GetRequiredService<ILogger<PostsMonitor>>()));
builder.Services.AddSingleton(sp => new CommentsMonitor(sp.GetRequiredService<ISubredditFactory>().Create(config.Subreddit), sp.GetRequiredService<InMemoryDb>(), sp.GetRequiredService<ILogger<CommentsMonitor>>()));

var app = builder.Build();

// Start monitors
var postsMonitor = app.Services.GetRequiredService<PostsMonitor>();
postsMonitor.Start(config.MonitoringInterval);

var commentsMonitor = app.Services.GetRequiredService<CommentsMonitor>();
commentsMonitor.Start(config.MonitoringInterval);

// Using minimal API pattern
// Adding API methods for supported stats. To extend support for new stats new API methods may need to be added.
app.MapGet("/getrankedposts", async ([FromServices] QueryErrorDecorator<RedditPost> query) => TypedResults.Ok(await query.GetAsync().ConfigureAwait(false)));

app.MapGet("/getrankedusers", async ([FromServices] QueryErrorDecorator<RedditUser> query) => TypedResults.Ok(await query.GetAsync().ConfigureAwait(false)));

app.MapGet("/getrankedcomments", async ([FromServices] QueryErrorDecorator<RedditComment> query) => TypedResults.Ok(await query.GetAsync().ConfigureAwait(false)));

app.MapGet("/", () => $"RedditStats.WebAPI has started!");

await app.RunAsync().ConfigureAwait(false);