using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using QPhising.Application.Common.Abstractions;
using QPhising.Application.Features.Analytics.GetDashboardKpis;
using QPhising.Domain.Campaigns;
using StackExchange.Redis;

namespace QPhising.Infrastructure.Persistence;

public sealed class RedisAnalyticsDashboardCache(
    IConnectionMultiplexer connectionMultiplexer,
    IOptions<RedisOptions> redisOptions) : IAnalyticsDashboardCache
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly IDatabase _database = connectionMultiplexer.GetDatabase();
    private readonly RedisOptions _options = redisOptions.Value;

    public async Task<DashboardKpisResponse?> GetAsync(GetDashboardKpisQuery query, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        long generation = await GetGenerationAsync().ConfigureAwait(false);
        string cacheKey = BuildCacheKey(query, generation);

        RedisValue payload = await _database.StringGetAsync(cacheKey).ConfigureAwait(false);
        if (payload.IsNullOrEmpty)
        {
            return null;
        }

        return JsonSerializer.Deserialize<DashboardKpisResponse>(payload.ToString(), SerializerOptions);
    }

    public async Task SetAsync(
        GetDashboardKpisQuery query,
        DashboardKpisResponse response,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        long generation = await GetGenerationAsync().ConfigureAwait(false);
        string cacheKey = BuildCacheKey(query, generation);
        string payload = JsonSerializer.Serialize(response, SerializerOptions);
        TimeSpan ttl = TimeSpan.FromSeconds(_options.AnalyticsDashboardCacheTtlSeconds);

        await _database.StringSetAsync(cacheKey, payload, ttl).ConfigureAwait(false);
    }

    public async Task InvalidateAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _database.StringIncrementAsync(GetGenerationKey()).ConfigureAwait(false);
    }

    private async Task<long> GetGenerationAsync()
    {
        RedisValue value = await _database.StringGetAsync(GetGenerationKey()).ConfigureAwait(false);
        return value.IsNullOrEmpty ? 0 : (long)value;
    }

    private string BuildCacheKey(GetDashboardKpisQuery query, long generation)
    {
        string canonicalFilter = BuildCanonicalFilter(query);
        string fingerprint = ToSha256(canonicalFilter);
        return $"{_options.KeyPrefix}:analytics:dashboard:g{generation}:{fingerprint}";
    }

    private string GetGenerationKey() => $"{_options.KeyPrefix}:analytics:dashboard:generation";

    private static string BuildCanonicalFilter(GetDashboardKpisQuery query)
    {
        IEnumerable<string> campaignIds = (query.CampaignIds ?? Array.Empty<Guid>())
            .Distinct()
            .OrderBy(id => id)
            .Select(id => id.ToString("D"));
        IEnumerable<string> templateTypes = (query.TemplateTypes ?? Array.Empty<TemplateType>())
            .Distinct()
            .OrderBy(type => type)
            .Select(type => type.ToString());
        IEnumerable<string> campaignStatuses = (query.CampaignStatuses ?? Array.Empty<CampaignStatus>())
            .Distinct()
            .OrderBy(status => status)
            .Select(status => status.ToString());

        return string.Join(
            '|',
            query.From.ToUnixTimeSeconds(),
            query.To.ToUnixTimeSeconds(),
            query.TimeGrain,
            query.TimeZone.Trim(),
            string.Join(',', campaignIds),
            string.Join(',', templateTypes),
            string.Join(',', campaignStatuses));
    }

    private static string ToSha256(string input)
    {
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hash);
    }
}
