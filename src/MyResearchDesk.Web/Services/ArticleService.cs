using Microsoft.EntityFrameworkCore;
using MyResearchDesk.Web.Data;
using MyResearchDesk.Web.Models;

namespace MyResearchDesk.Web.Services;

public class ArticleService(AppDbContext db)
{
    public Task<List<Article>> ListAsync() =>
        db.Articles.AsNoTracking().OrderByDescending(a => a.CreatedAt).ToListAsync();

    public Task<Article?> GetAsync(Guid id) =>
        db.Articles.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);

    public async Task<Article> CreateAsync(ArticleInput input)
    {
        var now = DateTime.UtcNow;
        var article = new Article { Id = Guid.NewGuid(), CreatedAt = now, UpdatedAt = now };
        Apply(article, input);
        db.Articles.Add(article);
        await db.SaveChangesAsync();
        return article;
    }

    public async Task<bool> UpdateAsync(Guid id, ArticleInput input)
    {
        var article = await db.Articles.FindAsync(id);
        if (article is null) return false;
        Apply(article, input);
        article.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var deleted = await db.Articles.Where(a => a.Id == id).ExecuteDeleteAsync();
        return deleted > 0;
    }

    public static ArticleInput ToInput(Article a) => new()
    {
        Title = a.Title, Url = a.Url, Topic = a.Topic,
        Source = a.Source, Status = a.Status, Takeaway = a.Takeaway
    };

    private static void Apply(Article a, ArticleInput i)
    {
        a.Title = i.Title.Trim();
        a.Url = i.Url.Trim();
        a.Topic = i.Topic.Trim();
        a.Source = string.IsNullOrWhiteSpace(i.Source) ? null : i.Source.Trim();
        a.Status = i.Status;
        a.Takeaway = i.Takeaway.Trim();
    }
}
