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
                CampaignClickCount: 0,
                RecipientClickCount: 0);
        }

        string campaignCounterKey = $"{_options.KeyPrefix}:tracking:counter:campaign:{request.CampaignId:D}";
        string recipientCounterKey = $"{_options.KeyPrefix}:tracking:counter:recipient:{recipientKey}";

        long campaignCounter = await _database.StringIncrementAsync(campaignCounterKey).ConfigureAwait(false);
        long recipientCounter = await _database.StringIncrementAsync(recipientCounterKey).ConfigureAwait(false);

        return new TrackingClickRealtimeResult(
            IsDuplicate: false,
            CampaignClickCount: campaignCounter,
            RecipientClickCount: recipientCounter);
    }
}
