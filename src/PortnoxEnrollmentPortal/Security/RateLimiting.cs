using System.Threading.RateLimiting;

namespace PortnoxEnrollmentPortal.Security;

public static class PortnoxRateLimiting
{
    public static RateLimiter CreateLimiter()
    {
        return new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
        {
            TokenLimit = 10,
            TokensPerPeriod = 10,
            ReplenishmentPeriod = TimeSpan.FromSeconds(10),
            AutoReplenishment = true,
            QueueLimit = 100,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
        });
    }
}
