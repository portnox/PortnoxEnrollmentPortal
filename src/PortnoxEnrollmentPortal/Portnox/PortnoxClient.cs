using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using System.Threading.RateLimiting;
using Microsoft.Net.Http.Headers;

namespace PortnoxEnrollmentPortal.Portnox;

public class PortnoxClient
{
    private readonly HttpClient _http;
    private readonly PortnoxOptions _opt;
    private readonly RateLimiter _limiter;

    public PortnoxClient(HttpClient http, IOptions<PortnoxOptions> opt)
    {
        _http = http;
        _opt = opt.Value;

        _http.Timeout = TimeSpan.FromSeconds(Math.Max(5, _opt.TimeoutSeconds));
    }

    public async Task<PortnoxResult> CreateAccountAsync(string upn, string displayName, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_opt.CreateAccountUrl))
            throw new InvalidOperationException("Portnox:CreateAccountUrl is required.");

        if (string.IsNullOrWhiteSpace(_opt.ApiKey))
            throw new InvalidOperationException("Portnox:ApiKey is required (use env var/secret store).");

        var url = _opt.CreateAccountUrl;

        var payload = new CreateAccountRequest(
            [new ClearAccounts(upn, displayName)]
        );

        var json = JsonSerializer.Serialize(payload);
        using var req = new HttpRequestMessage(HttpMethod.Post, url);
        req.Content = new StringContent(json, Encoding.UTF8, "application/json");

        // Most Portnox tenants use an apiKey header; adjust if your Swagger says otherwise.
        req.Headers.Add(HeaderNames.Authorization, $"Bearer {_opt.ApiKey}");

        using var resp = await _http.SendAsync(req, ct);
        var body = await resp.Content.ReadAsStringAsync(ct);
        var snippet = body.Length > 2000 ? body.Substring(0, 2000) : body;

        // Idempotency: treat "already exists" as success.
        if ((int)resp.StatusCode == 409 || body.Contains("already", StringComparison.OrdinalIgnoreCase))
            return new PortnoxResult(true, (int)resp.StatusCode, snippet);

        return new PortnoxResult(resp.IsSuccessStatusCode, (int)resp.StatusCode, snippet);
    }

    private static string Combine(string baseUrl, string path)
    {
        baseUrl = baseUrl.TrimEnd('/');
        path = "/" + path.TrimStart('/');
        return baseUrl + path;
    }
}
