using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyResearchDesk.Web.Models;
using MyResearchDesk.Web.Services;

namespace MyResearchDesk.Web.Pages.Articles;

public class CreateModel(ArticleService service) : PageModel
{
    [BindProperty]
    public ArticleInput Input { get; set; } = new();

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        await service.CreateAsync(Input);
        return RedirectToPage("Index");
    }
}
