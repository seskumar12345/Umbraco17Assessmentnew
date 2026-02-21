using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Web.Website.Controllers;

namespace Umbraco17Assessmentnew.Controllers;

/// <summary>
/// Handles the newsletter sign-up form rendered on Article pages.
/// POST → /umbraco/surface/newsletter/subscribe
/// </summary>
public sealed  class NewsletterSurfaceController : SurfaceController
{
    public NewsletterSurfaceController(
        Umbraco.Cms.Core.Web.IUmbracoContextAccessor umbracoContextAccessor,
        Umbraco.Cms.Infrastructure.Persistence.IUmbracoDatabaseFactory databaseFactory,
        Umbraco.Cms.Core.Services.ServiceContext serviceContext,
        Umbraco.Cms.Core.Cache.AppCaches appCaches,
        Umbraco.Cms.Core.Logging.IProfilingLogger profilingLogger,
        Umbraco.Cms.Core.Routing.IPublishedUrlProvider publishedUrlProvider)
        : base(umbracoContextAccessor, databaseFactory, serviceContext, appCaches, profilingLogger, publishedUrlProvider)
    { }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Subscribe(NewsletterModel model)
    {
        if (!ModelState.IsValid)
            return CurrentUmbracoPage();

        // TODO: plug in real subscription logic (Mailchimp, database, etc.)
        TempData["NewsletterSuccess"] = true;
        return RedirectToCurrentUmbracoPage();
    }
}

public sealed class NewsletterModel
{
    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    public string? Email { get; set; }
}
