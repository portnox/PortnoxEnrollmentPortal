using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using PortnoxEnrollmentPortal.Data;
using PortnoxEnrollmentPortal.Directory;
using PortnoxEnrollmentPortal.Portnox;
using PortnoxEnrollmentPortal.Security;

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

// Db (SQLite for simplicity; swap to SQL Server easily)
builder.Services.AddDbContext<AppDbContext>(opt =>
{
    var cs = builder.Configuration.GetConnectionString("Default")
             ?? "Data Source=app.db";
    opt.UseSqlite(cs);
});

builder.Services.Configure<PortnoxOptions>(builder.Configuration.GetSection("Portnox"));
builder.Services.AddHttpClient<PortnoxClient>();

builder.Services.AddSingleton<AdIdentityResolver>();

// Portnox rate limit: 10 requests / 10 seconds
builder.Services.AddSingleton(PortnoxRateLimiting.CreateLimiter());

var app = builder.Build();

app.UseForwardedHeaders();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

// Apply migrations on startup (optional; for prod you may prefer explicit migration)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();
