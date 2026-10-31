namespace MyResearchDesk.Web.Models;

public class Article
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public string Topic { get; set; } = string.Empty;

    public string? Source { get; set; }

    public string Takeaway { get; set; } = string.Empty;

    public ArticleStatus Status { get; set; } = ArticleStatus.ToRead;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
