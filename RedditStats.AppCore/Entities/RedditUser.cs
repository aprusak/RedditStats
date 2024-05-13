using Microsoft.AspNetCore.Http.HttpResults;

namespace RedditStats.AppCore.Entities
{
    public class RedditUser
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public int TotalPosts { get; set; }

        public int TotalComments { get; set; }

        public override string ToString()
        {
            return $"{{ Id:`{Id}`, Name:`{Name}`, TotalPosts:`{TotalPosts}`, TotalComments:`{TotalComments}` }}";
        }
    }
}
