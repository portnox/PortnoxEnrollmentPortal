using System.DirectoryServices.AccountManagement;
using System.Security.Principal;

namespace PortnoxEnrollmentPortal.Directory;

public record ResolvedIdentity(
    string DomainSam,
    string Upn,
    string Sid,
    string AdObjectGuid,
    string DisplayName
);

public class AdIdentityResolver
{
    public ResolvedIdentity ResolveFromHttpUser(string domainSam)
    {
        var parts = domainSam.Split('\\');
        var sam = parts.Length == 2 ? parts[1] : domainSam;

        using var ctx = new PrincipalContext(ContextType.Domain);
        var user = UserPrincipal.FindByIdentity(ctx, IdentityType.SamAccountName, sam);

        if (user is null)
            throw new InvalidOperationException($"Unable to find AD user for {domainSam}");

        var upn = user.UserPrincipalName ?? "";
        if (string.IsNullOrWhiteSpace(upn))
            throw new InvalidOperationException($"UPN not present for {domainSam}");

        var sid = user.Sid?.Value;
        var guid = user.Guid?.ToString("N") ?? "";

        return new ResolvedIdentity(
            DomainSam: domainSam,
            Upn: upn,
            Sid: sid,
            AdObjectGuid: guid,
            DisplayName: user.DisplayName ?? sam
        );
    }
}
