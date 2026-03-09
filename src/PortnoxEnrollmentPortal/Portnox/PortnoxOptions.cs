namespace PortnoxEnrollmentPortal.Portnox;

public class PortnoxOptions
{
    public string BaseUrl { get; set; } = "";
    public string ApiKey { get; set; } = "";
    public string CreateAccountPath { get; set; } = "/api/account/create";
    public int TimeoutSeconds { get; set; } = 30;
}
