namespace RedditStats.AppCore.Configuration
{
    public class Config
    {
        public required string AppId { get; set; }
        public required string RefreshToken { get; set; }
        public required string AccessToken { get; set; }
        public required string Subreddit { get; set; }
        public required TimeSpan MonitoringInterval { get; set; }
    }
}
