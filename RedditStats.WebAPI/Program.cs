using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedditStats.AppCore.Configuration;
using RedditStats.AppCore.Entities;
using RedditStats.Infra.Monitors;
using RedditStats.Infra.Reddit;
using RedditStats.Infra.Storage;

var builder = WebApplication.CreateBuilder(args);

// Read and bind config section
var section = builder.Configuration.GetRequiredSection("Config");
var config = section.Get<Config>() ?? throw new InvalidOperationException($"Can't bind configuration instance to the following instance type: `{nameof(Config)}`");

builder.Services.AddLogging();

// Register in-memory-db
builder.Services.AddDbContext<DbContext, InMemoryDb>(options => options.UseInMemoryDatabase("in-memory-db"), ServiceLifetime.Transient, ServiceLifetime.Transient);

// Register Repository
builder.Services.AddTransient<IRepository, Repository>();

// Register Subreddit Factory
builder.Services.AddSingleton<ISubredditFactory, SubredditFactory>(sp => new SubredditFactory(config.AppId, config.AccessToken, config.RefreshToken, sp.GetRequiredService<ILogger<SubredditFactory>>()));

// Register monitors. To extend support for new stats new monitors may need to be added.
builder.Services.AddSingleton(sp => new PostsMonitor(sp.GetRequiredService<ISubredditFactory>().Create(config.Subreddit), sp.GetRequiredService<IRepository>(), sp.GetRequiredService<ILogger<PostsMonitor>>()));
builder.Services.AddSingleton(sp => new CommentsMonitor(sp.GetRequiredService<ISubredditFactory>().Create(config.Subreddit), sp.GetRequiredService<IRepository>(), sp.GetRequiredService<ILogger<CommentsMonitor>>()));

var app = builder.Build();

// Start monitors
var postsMonitor = app.Services.GetRequiredService<PostsMonitor>();
postsMonitor.Start(config.MonitoringInterval);

var commentsMonitor = app.Services.GetRequiredService<CommentsMonitor>();
commentsMonitor.Start(config.MonitoringInterval);

if (app.Environment.IsDevelopment()) app.UseDeveloperExceptionPage();
else app.UseExceptionHandler("/error");

// Using minimal API pattern
// Adding API methods for supported stats. To extend support for new stats new API methods may need to be added.
app.MapGet("/GetPosts", async ([FromServices] IRepository repository) => TypedResults.Ok((await repository.GetAsync<RedditPost>().ConfigureAwait(false)).OrderByDescending(p => p.UpVotes)));

app.MapGet("/GetUsers", async ([FromServices] IRepository repository) => TypedResults.Ok((await repository.GetAsync<RedditUser>().ConfigureAwait(false)).OrderByDescending(u => u.TotalPosts + u.TotalComments)));

app.MapGet("/GetComments", async ([FromServices] IRepository repository) => TypedResults.Ok((await repository.GetAsync<RedditComment>().ConfigureAwait(false)).OrderByDescending(c => c.UpVotes)));

app.MapGet("/", () => $"RedditStats.WebAPI has started!");

await app.RunAsync().ConfigureAwait(false);