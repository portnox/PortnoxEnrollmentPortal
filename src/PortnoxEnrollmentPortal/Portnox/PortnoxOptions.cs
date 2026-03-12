namespace PortnoxEnrollmentPortal.Portnox;

public class PortnoxOptions
{

    public string ApiKey { get; set; } = "";

    public string CreateAccountUrl { get; set; } = "https://cloud-portal.portnox.com:8081/CloudPortalBackEnd/api/cloud-accounts";

    public int TimeoutSeconds { get; set; } = 30;
}
