using Microsoft.AspNetCore.Mvc.RazorPages;
using MyResearchDesk.Web.Models;
using MyResearchDesk.Web.Services;

namespace MyResearchDesk.Web.Pages.Articles;

public class IndexModel(ArticleService service) : PageModel
{
    public List<Article> Articles { get; private set; } = [];

    public async Task OnGetAsync() => Articles = await service.ListAsync();
}
