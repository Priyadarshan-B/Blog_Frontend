
    public class Comment
    {
        public string? Id { get; set; }
        public string PostId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

