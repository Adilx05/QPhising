using Microsoft.Extensions.Options;
using QPhising.Application.Common.Abstractions;
using StackExchange.Redis;

namespace QPhising.Infrastructure.Persistence;

public sealed class RedisTrackingClickRealtimeStore(
    IConnectionMultiplexer connectionMultiplexer,
    IOptions<RedisOptions> redisOptions) : ITrackingClickRealtimeStore
{
    private readonly IDatabase _database = connectionMultiplexer.GetDatabase();
    private readonly RedisOptions _options = redisOptions.Value;

    public async Task<TrackingClickRealtimeResult> RegisterClickAsync(
        TrackingClickRealtimeRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var allowedClockSkew = TimeSpan.FromSeconds(_options.TrackingTokenClockSkewSeconds);
        if (request.ClickedAtUtc < request.TokenIssuedAtUtc.Subtract(allowedClockSkew) ||
            request.ClickedAtUtc > request.TokenExpiresAtUtc.Add(allowedClockSkew))
        {
            return new TrackingClickRealtimeResult(
                IsDuplicate: false,
                IsRejected: true,
                IsFlagged: true,
                DecisionReason: "outside_valid_window",
                CampaignClickCount: 0,
                RecipientClickCount: 0);
        }

        string recipientKey = request.RecipientEmail.Trim().ToLowerInvariant();
        string dedupKey = $"{_options.KeyPrefix}:tracking:dedup:{request.CampaignId:D}:{recipientKey}:{request.TokenNonce}";
        var dedupWindow = TimeSpan.FromMinutes(_options.TrackingDeduplicationWindowMinutes);

        bool registered = await _database.StringSetAsync(
            dedupKey,
            request.ClickedAtUtc.ToUnixTimeSeconds(),
            expiry: dedupWindow,
            when: When.NotExists).ConfigureAwait(false);

        if (!registered)
        {
            return new TrackingClickRealtimeResult(
                IsDuplicate: true,
                IsRejected: false,
                IsFlagged: true,
                DecisionReason: "duplicate_nonce",
                CampaignClickCount: 0,
                RecipientClickCount: 0);
        }

        var abuseWindow = TimeSpan.FromMinutes(_options.TrackingAbuseWindowMinutes);
        string sanitizedIp = request.IpAddress.Trim().ToLowerInvariant();
        string ipRateKey = $"{_options.KeyPrefix}:tracking:abuse:ip:{request.CampaignId:D}:{sanitizedIp}";
        long ipRateCounter = await IncrementWindowedCounterAsync(ipRateKey, abuseWindow).ConfigureAwait(false);

        if (ipRateCounter > _options.TrackingIpRejectionThreshold)
        {
            return new TrackingClickRealtimeResult(
                IsDuplicate: false,
                IsRejected: true,
                IsFlagged: true,
                DecisionReason: "ip_threshold_exceeded",
                CampaignClickCount: 0,
                RecipientClickCount: 0);
        }

        bool isFlagged = ipRateCounter > _options.TrackingSuspiciousIpThreshold;
        string? decisionReason = isFlagged ? "ip_rate_suspicious" : null;

        string campaignCounterKey = $"{_options.KeyPrefix}:tracking:counter:campaign:{request.CampaignId:D}";
        string recipientCounterKey = $"{_options.KeyPrefix}:tracking:counter:recipient:{recipientKey}";
        var counterRetention = TimeSpan.FromDays(Math.Max(1, _options.TrackingAggregateRetentionDays));

        long campaignCounter = await _database.StringIncrementAsync(campaignCounterKey).ConfigureAwait(false);
        long recipientCounter = await _database.StringIncrementAsync(recipientCounterKey).ConfigureAwait(false);

        if (campaignCounter == 1)
        {
            await _database.KeyExpireAsync(campaignCounterKey, counterRetention).ConfigureAwait(false);
        }

        if (recipientCounter == 1)
        {
            await _database.KeyExpireAsync(recipientCounterKey, counterRetention).ConfigureAwait(false);
        }

        return new TrackingClickRealtimeResult(
            IsDuplicate: false,
            IsRejected: false,
            IsFlagged: isFlagged,
            DecisionReason: decisionReason,
            CampaignClickCount: campaignCounter,
            RecipientClickCount: recipientCounter);
    }

    private async Task<long> IncrementWindowedCounterAsync(string key, TimeSpan window)
    {
        long count = await _database.StringIncrementAsync(key).ConfigureAwait(false);
        if (count == 1)
        {
            await _database.KeyExpireAsync(key, window).ConfigureAwait(false);
        }

        return count;
    }
}
