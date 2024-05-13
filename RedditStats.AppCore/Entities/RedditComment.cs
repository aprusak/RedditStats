namespace RedditStats.AppCore.Entities
{
    public class RedditComment
    {
        public string? Id { get; set; }

        public string? PostId { get; set; }

        public DateTime Created { get; set; }

        public string? Author { get; set; }

        public int? NumReplies { get; set; }

        public int Score { get; set; }

        public string? Permalink { get; set; }

        public int UpVotes { get; set; }

        public override string ToString()
        {
            return $"{{ Id:`{Id}`, PostId:`{PostId}`, Author:`{Author}`, Created:`{Created}`, NumReplies:`{NumReplies}`, UpVotes:`{UpVotes}`, Score:`{Score}`, Permalink:`{Permalink}` }}";
        }
    }
}
