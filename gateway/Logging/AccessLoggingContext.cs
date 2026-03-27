namespace Gateway.Logging;

public static class AccessLoggingContext
{
    public const string RateLimitAppliedKey = "AccessLog:RateLimitApplied";
    public const string RateLimitExceededKey = "AccessLog:RateLimitExceeded";
    public const string RateLimitRuleKey = "AccessLog:RateLimitRule";
}
