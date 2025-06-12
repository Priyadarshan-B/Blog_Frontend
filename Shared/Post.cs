public class Post
{
    public string? Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;

    public List<string> Preferences { get; set; } = new List<string>();
    public int Likes { get; set; }
    public int CommentCount { get; set; }
}
