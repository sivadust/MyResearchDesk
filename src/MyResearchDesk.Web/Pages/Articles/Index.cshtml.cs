using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyResearchDesk.Web.Models;
using MyResearchDesk.Web.Services;

namespace MyResearchDesk.Web.Pages.Articles;

public class IndexModel(ArticleService service) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? Q { get; set; }

    [BindProperty(SupportsGet = true)]
    public ArticleStatus? Status { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Topic { get; set; }

    public List<Article> Articles { get; private set; } = [];

    public List<string> Topics { get; private set; } = [];

    public bool IsFiltered => !string.IsNullOrWhiteSpace(Q) || Status is not null || !string.IsNullOrWhiteSpace(Topic);

    public async Task OnGetAsync()
    {
        if (Status is not null && !Enum.IsDefined(Status.Value)) Status = null;
        Articles = await service.SearchAsync(Q, Status, Topic);
        Topics = await service.TopicsAsync();
    }
}
