# Portnox Enrollment Portal (IIS + Kerberos + AD)

Centralized internal web portal that uses Windows Authentication (Kerberos/Negotiate) to identify a user, resolves their UPN from Active Directory, and on explicit **Enroll** click calls Portnox Cloud REST API to create a Portnox Account.

## Prereqs
- Windows Server (domain-joined)
- IIS installed + ASP.NET Core Hosting Bundle (for .NET 8)
- Portnox Cloud tenant API token

## Configure IIS
- Site: HTTPS (internal cert)
- Authentication:
  - Enable **Windows Authentication**
  - Disable **Anonymous Authentication**
  - Providers: **Negotiate** above **NTLM** (prefer Kerberos)

## Configure Portnox
Generate an API token in your Portnox tenant UI.
Set the API key securely (recommended: environment variable) and do **not** commit it.

### appsettings.json
Update:
- `Portnox:BaseUrl`
- `Portnox:CreateAccountPath` (copy from your tenant Swagger: Help -> API Reference)
- Adjust the request payload in `src/PortnoxEnrollmentPortal/Portnox/PortnoxModels.cs` to match your tenant schema.

## Secrets
Recommended:
- Set `Portnox__ApiKey` as an environment variable on the IIS server.

### PowerShell
```powershell
setx Portnox__ApiKey "YOUR_TOKEN_VALUE" /M
```
Restart IIS after setting.

## Run locally (dev)
```powershell
cd src\PortnoxEnrollmentPortal
# Install EF tools if needed
 dotnet tool update --global dotnet-ef
# Create database
 dotnet ef migrations add InitialCreate
 dotnet ef database update
# Run
 dotnet run
```

## Deploy
Publish:
```powershell
dotnet publish -c Release
```
Copy output to IIS site folder.
