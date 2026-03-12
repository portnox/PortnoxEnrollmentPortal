using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PortnoxEnrollmentPortal.Directory;
using PortnoxEnrollmentPortal.Portnox;

namespace PortnoxEnrollmentPortal.Pages;

public class IndexModel : PageModel
{
    private readonly AdIdentityResolver _resolver;
    private readonly PortnoxClient _portnox;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(AdIdentityResolver resolver, PortnoxClient portnox, ILogger<IndexModel> logger)
    {
        _resolver = resolver;
        _portnox = portnox;
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

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        LoadIdentity();

        try
        {
            var result = await _portnox.CreateAccountAsync(Upn, DisplayName, ct);

            TempData["StatusMessage"] = result.Success
                ? $"✅ Enrolled {Upn} in Portnox. (HTTP {result.StatusCode})"
                : $"❌ Enrollment failed for {Upn}. (HTTP {result.StatusCode})";

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
        var resolved = _resolver.ResolveFromHttpUser(DomainSam);
        Upn = resolved.Upn;
        DisplayName = resolved.DisplayName;
    }
}
