namespace PortnoxEnrollmentPortal.Portnox;

public record CreateAccountRequest(
    ClearAccounts[] ClearAccounts
);

public record ClearAccounts(
    string? AccountName,
    string? Description = null,
    string? CredentialsExpirationDate = null,
    bool AllowAgentlessDevices = true
);

public record PortnoxResult(
    bool Success,
    int StatusCode,
    string BodySnippet
);
