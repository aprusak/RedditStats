namespace RedditStats.AppCore.Entities
{
    public class RedditPost
    {
        public int Id { get; set; }

        public string? PostId { get; set; }

        public DateTime Created { get; set; }

        public string? Title { get; set; }

        public string? Author { get; set; }

        public string? Permalink { get; set; }

        public int UpVotes { get; set; }

        public int Score { get; set; }

        public override string ToString()
        {
            return $"{{ Id:`{Id}`, PostId:`{PostId}`, Author:`{Author}`, Created:`{Created}`, Title:`{Title}`, UpVotes:`{UpVotes}`, Score:`{Score}`, Permalink:`{Permalink}` }}";
        }
    }
}
