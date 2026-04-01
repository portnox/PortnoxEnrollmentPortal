using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;
using PortnoxEnrollmentPortal.Directory;
using PortnoxEnrollmentPortal.Portnox;

var builder = WebApplication.CreateBuilder(args);

// IIS + Windows Auth (Kerberos via Negotiate provider ordering in IIS)
builder.Services
    .AddAuthentication(NegotiateDefaults.AuthenticationScheme)
    .AddNegotiate();

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/");
});

builder.Services.Configure<PortnoxOptions>(builder.Configuration.GetSection("Portnox"));
builder.Services.AddHttpClient<PortnoxClient>();
builder.Services.AddSingleton<AdIdentityResolver>();

var app = builder.Build();

app.UseForwardedHeaders();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
