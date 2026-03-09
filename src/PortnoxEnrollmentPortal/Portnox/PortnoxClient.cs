using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using System.Threading.RateLimiting;

namespace PortnoxEnrollmentPortal.Portnox;

public class PortnoxClient
{
    private readonly HttpClient _http;
    private readonly PortnoxOptions _opt;
    private readonly RateLimiter _limiter;

    public PortnoxClient(HttpClient http, IOptions<PortnoxOptions> opt, RateLimiter limiter)
    {
        _http = http;
        _opt = opt.Value;
        _limiter = limiter;

        _http.Timeout = TimeSpan.FromSeconds(Math.Max(5, _opt.TimeoutSeconds));
    }

    public async Task<PortnoxResult> CreateAccountAsync(string upn, string displayName, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_opt.BaseUrl))
            throw new InvalidOperationException("Portnox:BaseUrl is required.");

        if (string.IsNullOrWhiteSpace(_opt.ApiKey))
            throw new InvalidOperationException("Portnox:ApiKey is required (use env var/secret store).");

        using var lease = await _limiter.AcquireAsync(1, ct);
        if (!lease.IsAcquired)
            return new PortnoxResult(false, 429, "Local rate limiter denied request.");

        var url = Combine(_opt.BaseUrl, _opt.CreateAccountPath);

        var payload = new CreateAccountRequest(
            Email: upn,
            Description: displayName
        );

        var json = JsonSerializer.Serialize(payload);
        using var req = new HttpRequestMessage(HttpMethod.Post, url);
        req.Content = new StringContent(json, Encoding.UTF8, "application/json");

        // Most Portnox tenants use an apiKey header; adjust if your Swagger says otherwise.
        req.Headers.Add("apiKey", _opt.ApiKey);

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
