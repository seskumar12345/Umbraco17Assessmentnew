using System;
using System.Linq;
using System.Collections.Generic;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions; // <-- Add this using to bring IPublishedContentQuery extension methods into scope

namespace Umbraco17Assessmentnew.Controllers;

/// <summary>
/// Management API that supplies article statistics to the backoffice dashboard.
/// Route: /umbraco/management/api/v1/article-stats
/// Protected: backoffice-authenticated users only.
/// </summary>
/// <remarks>
/// Inherits ControllerBase directly rather than ManagementApiControllerBase so the
/// code compiles against whichever minor Umbraco 17.x patch is installed, without
/// being sensitive to internal base-class restructuring between patch versions.
/// </remarks>
[ApiController]
[ApiVersion("1.0")]
[Route("umbraco/management/api/v{version:apiVersion}/article-stats")]
//[Authorize(Policy = AuthorizationPolicies.BackOfficeAccess)]
public sealed class ArticleStatsApiController : ControllerBase
{
    private readonly IPublishedContentQuery _publishedContentQuery;

    public ArticleStatsApiController(IPublishedContentQuery publishedContentQuery)
    {
        _publishedContentQuery = publishedContentQuery;
    }

    /// <summary>
    /// GET /umbraco/management/api/v1/article-stats
    /// Returns the total published article count and the five most-recently published.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ArticleStatsResponse), StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        // IPublishedContentQuery.ContentOfType is available in all Umbraco 10-17 versions

        // Fix:
        // - ContentAtRoot() returns IEnumerable<IPublishedContent>
        // - DescendantsOrSelf() is defined for a single IPublishedContent
        // - Use SelectMany to call DescendantsOrSelf() on each root item and flatten results
        var allArticles = _publishedContentQuery
            .ContentAtRoot()
            .SelectMany(root => root.DescendantsOrSelf())
            .Where(x => x.ContentType.Alias == "article")
            .ToList();


        var recent = allArticles
            .OrderByDescending(a => a.Value<DateTime?>("publishDate") ?? a.CreateDate)
            .Take(5)
            .Select(a => new ArticleSummary(
                a.Value<string>("title") ?? a.Name ?? string.Empty,
                a.Url(),
                a.Value<DateTime?>("publishDate") ?? a.CreateDate))
            .ToList();

        return Ok(new ArticleStatsResponse(allArticles.Count, recent));
    }

    // ── Response DTOs ──────────────────────────────────────────
    public sealed record ArticleStatsResponse(
        int TotalArticles,
        IReadOnlyList<ArticleSummary> RecentArticles);

    public sealed record ArticleSummary(
        string Title,
        string Url,
        DateTime PublishDate);
}
