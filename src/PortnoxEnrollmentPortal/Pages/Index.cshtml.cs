using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PortnoxEnrollmentPortal.Data;
using PortnoxEnrollmentPortal.Directory;
using PortnoxEnrollmentPortal.Models;
using PortnoxEnrollmentPortal.Portnox;

namespace PortnoxEnrollmentPortal.Pages;

public class IndexModel : PageModel
{
    private readonly AdIdentityResolver _resolver;
    private readonly PortnoxClient _portnox;
    private readonly AppDbContext _db;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(AdIdentityResolver resolver, PortnoxClient portnox, AppDbContext db, ILogger<IndexModel> logger)
    {
        _resolver = resolver;
        _portnox = portnox;
        _db = db;
        _logger = logger;
    }

    public string DomainSam { get; private set; } = "";
    public string Upn { get; private set; } = "";
    public string DisplayName { get; private set; } = "";
    public string StatusMessage { get; private set; } = "";

    public void OnGet()
    {
        LoadIdentity();
        StatusMessage = TempData["StatusMessage"]?.ToString() ?? "";
    }

    public async Task<IActionResult> OnPostEnrollAsync(CancellationToken ct)
    {
        LoadIdentity();

        try
        {
            var resolved = _resolver.ResolveFromHttpUser(DomainSam, User.Identity);
            var result = await _portnox.CreateAccountAsync(resolved.Upn, resolved.DisplayName, ct);

            _db.Enrollments.Add(new EnrollmentRecord
            {
                Upn = resolved.Upn,
                DomainSam = resolved.DomainSam,
                Sid = resolved.Sid,
                AdObjectGuid = resolved.AdObjectGuid,
                EnrolledAtUtc = DateTime.UtcNow,
                PortnoxStatusCode = result.StatusCode,
                PortnoxResponseSnippet = result.BodySnippet
            });

            await _db.SaveChangesAsync(ct);

            TempData["StatusMessage"] = result.Success
                ? $"✅ Enrolled {resolved.Upn} in Portnox. (HTTP {result.StatusCode})"
                : $"❌ Enrollment failed for {resolved.Upn}. (HTTP {result.StatusCode})";

            return RedirectToPage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Enrollment error for {User}", DomainSam);
            TempData["StatusMessage"] = $"❌ Enrollment error: {ex.Message}";
            return RedirectToPage();
        }
    }

    private void LoadIdentity()
    {
        DomainSam = User?.Identity?.Name ?? "UNKNOWN";
        var resolved = _resolver.ResolveFromHttpUser(DomainSam, User.Identity);
        Upn = resolved.Upn;
        DisplayName = resolved.DisplayName;
    }
}
