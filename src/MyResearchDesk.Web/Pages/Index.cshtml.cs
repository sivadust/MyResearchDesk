using Microsoft.AspNetCore.Mvc.RazorPages;
using MyResearchDesk.Web.Models;
using MyResearchDesk.Web.Services;

namespace MyResearchDesk.Web.Pages;

public class IndexModel(ArticleService service) : PageModel
{
    public DashboardSummary Summary { get; private set; } = new(0, 0, 0, 0);

    public List<Article> Recent { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Summary = await service.GetSummaryAsync();
        Recent = await service.RecentAsync(5);
    }
}
