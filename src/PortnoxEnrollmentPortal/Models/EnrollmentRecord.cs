using System.ComponentModel.DataAnnotations;

namespace PortnoxEnrollmentPortal.Models;

public class EnrollmentRecord
{
    public long Id { get; set; }

    [MaxLength(256)]
    public string Upn { get; set; } = "";

    [MaxLength(256)]
    public string DomainSam { get; set; } = "";

    [MaxLength(256)]
    public string Sid { get; set; } = "";

    // Useful for internal tracking because UPN can change over time
    [MaxLength(64)]
    public string AdObjectGuid { get; set; } = "";

    public DateTime EnrolledAtUtc { get; set; }

    public int PortnoxStatusCode { get; set; }

    public string PortnoxResponseSnippet { get; set; } = "";
}
