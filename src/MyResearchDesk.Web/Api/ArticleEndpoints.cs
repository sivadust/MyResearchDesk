using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http.HttpResults;
using MyResearchDesk.Web.Models;
using MyResearchDesk.Web.Services;

namespace MyResearchDesk.Web.Api;

public static class ArticleEndpoints
{
    public static void MapArticleApi(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/articles").WithTags("Articles");

        group.MapGet("/", async (ArticleService svc, string? q, ArticleStatus? status, string? topic) =>
            TypedResults.Ok(await svc.SearchAsync(q, status, topic)))
            .WithSummary("List or search articles, newest first.");

        group.MapGet("/{id:guid}", async Task<Results<Ok<Article>, NotFound>> (Guid id, ArticleService svc) =>
            await svc.GetAsync(id) is { } a ? TypedResults.Ok(a) : TypedResults.NotFound())
            .WithSummary("Get one article.");

        group.MapPost("/", async Task<Results<Created<Article>, ValidationProblem>> (ArticleInput input, ArticleService svc) =>
        {
            if (Validate(input) is { } errors) return TypedResults.ValidationProblem(errors);
            var created = await svc.CreateAsync(input);
            return TypedResults.Created($"/api/articles/{created.Id}", created);
        }).WithSummary("Create an article.");

        group.MapPut("/{id:guid}", async Task<Results<NoContent, NotFound, ValidationProblem>> (Guid id, ArticleInput input, ArticleService svc) =>
        {
            if (Validate(input) is { } errors) return TypedResults.ValidationProblem(errors);
            return await svc.UpdateAsync(id, input) ? TypedResults.NoContent() : TypedResults.NotFound();
        }).WithSummary("Update an article.");

        group.MapDelete("/{id:guid}", async Task<Results<NoContent, NotFound>> (Guid id, ArticleService svc) =>
            await svc.DeleteAsync(id) ? TypedResults.NoContent() : TypedResults.NotFound())
            .WithSummary("Delete an article permanently.");
    }

    private static Dictionary<string, string[]>? Validate(ArticleInput input)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(input, new ValidationContext(input), results, validateAllProperties: true);
        var errors = results
            .SelectMany(r => (r.MemberNames.Any() ? r.MemberNames : [""]).Select(m => (m, r.ErrorMessage ?? "Invalid")))
            .GroupBy(x => x.m)
            .ToDictionary(g => g.Key, g => g.Select(x => x.Item2).ToArray());

        if (!Enum.IsDefined(input.Status))
            errors["Status"] = ["Status must be 1 (ToRead), 2 (Read) or 3 (WorthActingOn)."];

        return errors.Count > 0 ? errors : null;
    }
}
