using Microsoft.Extensions.Logging;
using Reddit;
using Reddit.Controllers;
using Reddit.Exceptions;

namespace RedditStats.Infra.Reddit
{
    /// <summary>
    /// Subreddit factory.
    /// Relies on https://github.com/sirkris/Reddit.NET.
    /// Corresponding Nuget package: https://www.nuget.org/packages/Reddit.
    /// </summary>
    public sealed class SubredditFactory : ISubredditFactory
    {
        readonly string _appId;
        readonly string _accessToken;
        readonly string _refreshToken;
        readonly ILogger<SubredditFactory> _logger;

        public SubredditFactory(string appId, string accessToken, string refreshToken, ILogger<SubredditFactory> logger)
        {
            if (string.IsNullOrWhiteSpace(appId)) throw new ArgumentNullException(nameof(appId));
            if (string.IsNullOrWhiteSpace(accessToken)) throw new ArgumentNullException(nameof(accessToken));
            if (string.IsNullOrWhiteSpace(refreshToken)) throw new ArgumentNullException(nameof(refreshToken));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _appId = appId;
            _accessToken = accessToken;
            _refreshToken = refreshToken;
        }

        public Subreddit Create(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            try
            {
                var redditClient = new RedditClient(appId: _appId, accessToken: _accessToken, refreshToken: _refreshToken);

                return redditClient.Subreddit(name);
            }
            catch (RedditUnauthorizedException ex)
            {
                _logger.LogError(ex, "There is an authorization issue. Please check your credentials and restart.");

                throw;
            }
            catch (RedditServiceUnavailableException ex)
            {
                _logger.LogError(ex, "The Reddit Service is currently unavailable: Please try again later.");
                
                throw;
            }
            catch (RedditGatewayTimeoutException ex)
            {
                _logger.LogError(ex, "The Reddit Service gateway is currently timing out: Please try again later.");

                throw;
            }
            catch (RedditInternalServerErrorException ex)
            {
                _logger.LogError(ex, "The Reddit Service has an Internal Server issue: Please try again later.");

                throw;
            }
            catch (RedditException ex)
            {
                _logger.LogError(ex, "The Reddit Service has an issue: Please try again later.");

                throw;
            }
        }
    }
}
