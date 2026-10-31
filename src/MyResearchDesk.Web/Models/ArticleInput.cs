using System.ComponentModel.DataAnnotations;

namespace MyResearchDesk.Web.Models;

public class ArticleInput
{
    [Required, StringLength(300)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(2048), Url]
    public string Url { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Topic { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Source { get; set; }

    [Required]
    public ArticleStatus Status { get; set; } = ArticleStatus.ToRead;

    [Required, StringLength(4000)]
    public string Takeaway { get; set; } = string.Empty;
}
