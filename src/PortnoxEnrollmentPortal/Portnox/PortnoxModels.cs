namespace PortnoxEnrollmentPortal.Portnox;

// Adjust to match your tenant Swagger schema
public record CreateAccountRequest(
    string Email,
    string? Description = null
);

public record PortnoxResult(
    bool Success,
    int StatusCode,
    string BodySnippet
);
