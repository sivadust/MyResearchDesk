using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyResearchDesk.Web.Models;
using MyResearchDesk.Web.Services;

namespace MyResearchDesk.Web.Pages.Articles;

public class DeleteModel(ArticleService service) : PageModel
{
    public Article Article { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var article = await service.GetAsync(id);
        if (article is null) return NotFound();
        Article = article;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id)
    {
        await service.DeleteAsync(id);
        return RedirectToPage("Index");
    }
}
