using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyResearchDesk.Web.Models;
using MyResearchDesk.Web.Services;

namespace MyResearchDesk.Web.Pages.Articles;

public class EditModel(ArticleService service) : PageModel
{
    [BindProperty]
    public ArticleInput Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var article = await service.GetAsync(id);
        if (article is null) return NotFound();
        Input = ArticleService.ToInput(article);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id)
    {
        if (!ModelState.IsValid) return Page();
        if (!await service.UpdateAsync(id, Input)) return NotFound();
        return RedirectToPage("Index");
    }
}
