using Reddit.Controllers;

namespace RedditStats.Infra.Reddit
{
    public interface ISubredditFactory
    {
        Subreddit Create(string name);
    }
}
