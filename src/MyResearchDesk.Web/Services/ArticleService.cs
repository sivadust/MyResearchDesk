using Microsoft.EntityFrameworkCore;
using MyResearchDesk.Web.Data;
using MyResearchDesk.Web.Models;

namespace MyResearchDesk.Web.Services;

public record DashboardSummary(int Total, int ToRead, int Read, int WorthActingOn);

public class ArticleService(AppDbContext db)
{
    public Task<List<Article>> ListAsync() =>
        db.Articles.AsNoTracking().OrderByDescending(a => a.CreatedAt).ToListAsync();

    public Task<List<Article>> SearchAsync(string? q, ArticleStatus? status, string? topic)
    {
        var query = db.Articles.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var escaped = q.Trim().Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
            var pattern = $"%{escaped}%";
            query = query.Where(a =>
                EF.Functions.ILike(a.Title, pattern, "\\") ||
                EF.Functions.ILike(a.Topic, pattern, "\\") ||
                (a.Source != null && EF.Functions.ILike(a.Source, pattern, "\\")) ||
                EF.Functions.ILike(a.Takeaway, pattern, "\\"));
        }

        if (status is not null) query = query.Where(a => a.Status == status);
        if (!string.IsNullOrWhiteSpace(topic)) query = query.Where(a => a.Topic == topic);

        return query.OrderByDescending(a => a.CreatedAt).ToListAsync();
    }

    public Task<List<string>> TopicsAsync() =>
        db.Articles.AsNoTracking().Select(a => a.Topic).Distinct().OrderBy(t => t).ToListAsync();

    public async Task<DashboardSummary> GetSummaryAsync()
    {
        var counts = await db.Articles.AsNoTracking()
            .GroupBy(a => a.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();
        int Of(ArticleStatus s) => counts.FirstOrDefault(c => c.Status == s)?.Count ?? 0;
        return new DashboardSummary(counts.Sum(c => c.Count), Of(ArticleStatus.ToRead),
            Of(ArticleStatus.Read), Of(ArticleStatus.WorthActingOn));
    }

    public Task<List<Article>> RecentAsync(int count) =>
        db.Articles.AsNoTracking().OrderByDescending(a => a.CreatedAt).Take(count).ToListAsync();

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
