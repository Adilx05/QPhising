using Microsoft.Extensions.Options;
using QPhising.Application.Common;
using QPhising.Application.Common.Abstractions;
using QPhising.Application.Features.Tracking.GenerateTrackingLink;
using QPhising.Application.Features.Tracking.ProcessTrackingClick;
using QPhising.Domain.Abstractions;
using QPhising.Domain.Campaigns;
using QPhising.Infrastructure.Security;
using Xunit;

namespace QPhising.API.IntegrationTests;

public sealed class TrackingInteractionGuardTests
{
    [Fact]
    public async Task GenerateTrackingLink_Should_Reject_When_Campaign_Is_Expired()
    {
        var now = DateTimeOffset.UtcNow;
        var campaign = Campaign.Create(
            "Expired Campaign",
            TemplateType.Email,
            "<h1>Phishing Simulation</h1>",
            now.AddDays(-10),
            now.AddDays(-1));

        var repository = new InMemoryCampaignRepository(campaign);
        var guard = new CampaignInteractionGuard(repository);
        var handler = new GenerateTrackingLinkCommandHandler(guard, CreateTokenService());

        var result = await handler.Handle(
            new GenerateTrackingLinkCommand(campaign.Id, "employee@company.test"),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("Tracking interactions are blocked", result.Errors.Single());
    }

    [Fact]
    public async Task ProcessTrackingClick_Should_Reject_When_Campaign_Is_Expired()
    {
        var now = DateTimeOffset.UtcNow;
        var campaign = Campaign.Create(
            "Expired Campaign",
            TemplateType.Email,
            "<h1>Phishing Simulation</h1>",
            now.AddDays(-5),
            now.AddDays(-1));

        var repository = new InMemoryCampaignRepository(campaign);
        var guard = new CampaignInteractionGuard(repository);
        var handler = new ProcessTrackingClickCommandHandler(
            guard,
            CreateTokenService(),
            new InMemoryTrackingClickRealtimeStore(),
            new InMemoryTrackingClickRepository(),
            new NoOpUnitOfWork());

        var result = await handler.Handle(
            new ProcessTrackingClickCommand(campaign.Id, "tracking-token", "127.0.0.1", "integration-test-agent"),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("Tracking interactions are blocked", result.Errors.Single());
    }

    [Fact]
    public async Task GenerateTrackingLink_Should_Succeed_When_Campaign_Is_Not_Expired()
    {
        var now = DateTimeOffset.UtcNow;
        var campaign = Campaign.Create(
            "Q3 Simulation",
            TemplateType.Email,
            "<h1>Phishing Simulation</h1>",
            now.AddDays(-1),
            now.AddDays(5));

        var repository = new InMemoryCampaignRepository(campaign);
        var guard = new CampaignInteractionGuard(repository);
        var tokenService = CreateTokenService();
        var handler = new GenerateTrackingLinkCommandHandler(guard, tokenService);

        var result = await handler.Handle(
            new GenerateTrackingLinkCommand(campaign.Id, "employee@company.test"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(campaign.Id, result.Value!.CampaignId);
        Assert.StartsWith("/api/v1/tracking/click/", result.Value.TrackingPath);
        Assert.Equal("HS256", result.Value.SignatureAlgorithm);

        var validation = tokenService.ValidateToken(result.Value.TrackingToken, campaign.Id);
        Assert.True(validation.IsValid);
        Assert.NotNull(validation.Claims);
        Assert.Equal("employee@company.test", validation.Claims!.RecipientEmail);
    }

    [Fact]
    public async Task ProcessTrackingClick_Should_Reject_When_TrackingToken_Is_Tampered()
    {
        var now = DateTimeOffset.UtcNow;
        var campaign = Campaign.Create(
            "Security Simulation",
            TemplateType.Email,
            "<h1>Simulation</h1>",
            now.AddDays(-1),
            now.AddDays(2));

        var repository = new InMemoryCampaignRepository(campaign);
        var guard = new CampaignInteractionGuard(repository);
        var tokenService = CreateTokenService();
        var issueResult = tokenService.IssueToken(new TrackingTokenIssueRequest(campaign.Id, "employee@company.test", Guid.NewGuid().ToString("N")));

        string tamperedToken = issueResult.Token[..^1] + (issueResult.Token[^1] == 'a' ? 'b' : 'a');
        var handler = new ProcessTrackingClickCommandHandler(
            guard,
            tokenService,
            new InMemoryTrackingClickRealtimeStore(),
            new InMemoryTrackingClickRepository(),
            new NoOpUnitOfWork());

        var result = await handler.Handle(
            new ProcessTrackingClickCommand(campaign.Id, tamperedToken, "127.0.0.1", "integration-test-agent"),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("signature is invalid", result.Errors.Single(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ProcessTrackingClick_Should_Be_Idempotent_For_Duplicate_Clicks()
    {
        var now = DateTimeOffset.UtcNow;
        var campaign = Campaign.Create(
            "Duplicate Guard Campaign",
            TemplateType.Email,
            "<h1>Simulation</h1>",
            now.AddDays(-1),
            now.AddDays(2));

        var repository = new InMemoryCampaignRepository(campaign);
        var guard = new CampaignInteractionGuard(repository);
        var tokenService = CreateTokenService();
        var issueResult = tokenService.IssueToken(new TrackingTokenIssueRequest(campaign.Id, "employee@company.test", Guid.NewGuid().ToString("N")));
        var clickRepository = new CountingTrackingClickRepository();
        var realtimeStore = new InMemoryTrackingClickRealtimeStore();
        var handler = new ProcessTrackingClickCommandHandler(
            guard,
            tokenService,
            realtimeStore,
            clickRepository,
            new NoOpUnitOfWork());

        var first = await handler.Handle(
            new ProcessTrackingClickCommand(campaign.Id, issueResult.Token, "127.0.0.1", "integration-test-agent"),
            CancellationToken.None);

        var second = await handler.Handle(
            new ProcessTrackingClickCommand(campaign.Id, issueResult.Token, "127.0.0.1", "integration-test-agent"),
            CancellationToken.None);

        Assert.True(first.IsSuccess);
        Assert.True(first.Value!.Accepted);
        Assert.True(second.IsSuccess);
        Assert.False(second.Value!.Accepted);
        Assert.True(second.Value.FlaggedForReview);
        Assert.Equal(Guid.Empty, second.Value.ClickId);
        Assert.Equal(1, clickRepository.AddedCount);
    }

    [Fact]
    public async Task ProcessTrackingClick_Should_Reject_When_Click_Is_Outside_Token_Time_Window()
    {
        var now = DateTimeOffset.UtcNow;
        var campaign = Campaign.Create(
            "Replay Guard Campaign",
            TemplateType.Email,
            "<h1>Simulation</h1>",
            now.AddDays(-1),
            now.AddDays(2));

        var repository = new InMemoryCampaignRepository(campaign);
        var guard = new CampaignInteractionGuard(repository);
        var tokenService = CreateTokenService();
        var issueResult = tokenService.IssueToken(new TrackingTokenIssueRequest(campaign.Id, "employee@company.test", Guid.NewGuid().ToString("N")));
        var handler = new ProcessTrackingClickCommandHandler(
            guard,
            tokenService,
            new InMemoryTrackingClickRealtimeStore(),
            new InMemoryTrackingClickRepository(),
            new NoOpUnitOfWork());

        var clickAttempt = await handler.Handle(
            new ProcessTrackingClickCommand(
                campaign.Id,
                issueResult.Token,
                "127.0.0.1",
                "integration-test-agent",
                ClickedAtUtc: issueResult.ExpiresAtUtc.AddMinutes(2)),
            CancellationToken.None);

        Assert.False(clickAttempt.IsSuccess);
        Assert.Contains("outside_valid_window", clickAttempt.Errors.Single());
    }

    [Fact]
    public async Task ProcessTrackingClick_Should_Flag_When_Ip_Rate_Is_Suspicious()
    {
        var now = DateTimeOffset.UtcNow;
        var campaign = Campaign.Create(
            "Abuse Guard Campaign",
            TemplateType.Email,
            "<h1>Simulation</h1>",
            now.AddDays(-1),
            now.AddDays(2));

        var repository = new InMemoryCampaignRepository(campaign);
        var guard = new CampaignInteractionGuard(repository);
        var tokenService = CreateTokenService();
        var realtimeStore = new InMemoryTrackingClickRealtimeStore(suspiciousIpThreshold: 2, rejectionIpThreshold: 10);
        var clickRepository = new CountingTrackingClickRepository();
        var handler = new ProcessTrackingClickCommandHandler(
            guard,
            tokenService,
            realtimeStore,
            clickRepository,
            new NoOpUnitOfWork());

        await handler.Handle(
            new ProcessTrackingClickCommand(campaign.Id, tokenService.IssueToken(new TrackingTokenIssueRequest(campaign.Id, "employee-1@company.test", Guid.NewGuid().ToString("N"))).Token, "127.0.0.1", "integration-test-agent"),
            CancellationToken.None);
        await handler.Handle(
            new ProcessTrackingClickCommand(campaign.Id, tokenService.IssueToken(new TrackingTokenIssueRequest(campaign.Id, "employee-2@company.test", Guid.NewGuid().ToString("N"))).Token, "127.0.0.1", "integration-test-agent"),
            CancellationToken.None);
        var suspiciousAttempt = await handler.Handle(
            new ProcessTrackingClickCommand(campaign.Id, tokenService.IssueToken(new TrackingTokenIssueRequest(campaign.Id, "employee-3@company.test", Guid.NewGuid().ToString("N"))).Token, "127.0.0.1", "integration-test-agent"),
            CancellationToken.None);

        Assert.True(suspiciousAttempt.IsSuccess);
        Assert.True(suspiciousAttempt.Value!.Accepted);
        Assert.True(suspiciousAttempt.Value.FlaggedForReview);
        Assert.Equal(3, clickRepository.AddedCount);
    }

    [Fact]
    public async Task ProcessTrackingClick_Should_Reject_When_Ip_Rate_Exceeds_Hard_Threshold()
    {
        var now = DateTimeOffset.UtcNow;
        var campaign = Campaign.Create(
            "Abuse Reject Campaign",
            TemplateType.Email,
            "<h1>Simulation</h1>",
            now.AddDays(-1),
            now.AddDays(2));

        var repository = new InMemoryCampaignRepository(campaign);
        var guard = new CampaignInteractionGuard(repository);
        var tokenService = CreateTokenService();
        var realtimeStore = new InMemoryTrackingClickRealtimeStore(suspiciousIpThreshold: 1, rejectionIpThreshold: 2);
        var clickRepository = new CountingTrackingClickRepository();
        var handler = new ProcessTrackingClickCommandHandler(
            guard,
            tokenService,
            realtimeStore,
            clickRepository,
            new NoOpUnitOfWork());

        await handler.Handle(
            new ProcessTrackingClickCommand(campaign.Id, tokenService.IssueToken(new TrackingTokenIssueRequest(campaign.Id, "employee-1@company.test", Guid.NewGuid().ToString("N"))).Token, "127.0.0.1", "integration-test-agent"),
            CancellationToken.None);
        await handler.Handle(
            new ProcessTrackingClickCommand(campaign.Id, tokenService.IssueToken(new TrackingTokenIssueRequest(campaign.Id, "employee-2@company.test", Guid.NewGuid().ToString("N"))).Token, "127.0.0.1", "integration-test-agent"),
            CancellationToken.None);
        var rejectedAttempt = await handler.Handle(
            new ProcessTrackingClickCommand(campaign.Id, tokenService.IssueToken(new TrackingTokenIssueRequest(campaign.Id, "employee-3@company.test", Guid.NewGuid().ToString("N"))).Token, "127.0.0.1", "integration-test-agent"),
            CancellationToken.None);

        Assert.False(rejectedAttempt.IsSuccess);
        Assert.Contains("ip_threshold_exceeded", rejectedAttempt.Errors.Single());
        Assert.Equal(2, clickRepository.AddedCount);
    }

    private static ITrackingTokenService CreateTokenService()
    {
        var options = Options.Create(new TrackingTokenOptions
        {
            SigningKey = "integration-test-signing-key-minimum-32chars",
            ExpirationMinutes = 30,
            Version = 1
        });

        return new HmacTrackingTokenService(options);
    }

    private sealed class InMemoryCampaignRepository : ICampaignRepository
    {
        private readonly Dictionary<Guid, Campaign> _campaigns;

        public InMemoryCampaignRepository(params Campaign[] campaigns)
        {
            _campaigns = campaigns.ToDictionary(c => c.Id, c => c);
        }

        public Task<Campaign?> GetByIdAsync(Guid campaignId, CancellationToken cancellationToken = default)
        {
            _campaigns.TryGetValue(campaignId, out Campaign? campaign);
            return Task.FromResult(campaign);
        }

        public Task<IReadOnlyCollection<Campaign>> ListAsync(CampaignReadCriteria criteria, CancellationToken cancellationToken = default)
        {
            IReadOnlyCollection<Campaign> result = _campaigns.Values.ToArray();
            return Task.FromResult(result);
        }

        public Task<IReadOnlyCollection<Campaign>> ListOverlappingWindowAsync(
            DateTimeOffset windowStart,
            DateTimeOffset windowEnd,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyCollection<Campaign> result = _campaigns.Values
                .Where(campaign => campaign.StartDate <= windowEnd && campaign.EndDate >= windowStart)
                .ToArray();

            return Task.FromResult(result);
        }

        public Task AddAsync(Campaign campaign, CancellationToken cancellationToken = default)
        {
            _campaigns[campaign.Id] = campaign;
            return Task.CompletedTask;
        }

        public void Update(Campaign campaign)
        {
            _campaigns[campaign.Id] = campaign;
        }
    }

    private sealed class InMemoryTrackingClickRepository : ITrackingClickRepository
    {
        public Task AddAsync(QPhising.Domain.Tracking.TrackingClick click, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class CountingTrackingClickRepository : ITrackingClickRepository
    {
        public int AddedCount { get; private set; }

        public Task AddAsync(QPhising.Domain.Tracking.TrackingClick click, CancellationToken cancellationToken = default)
        {
            AddedCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class InMemoryTrackingClickRealtimeStore : ITrackingClickRealtimeStore
    {
        private readonly HashSet<string> _dedupKeys = [];
        private readonly Dictionary<string, int> _ipCounters = new(StringComparer.OrdinalIgnoreCase);
        private readonly int _suspiciousIpThreshold;
        private readonly int _rejectionIpThreshold;

        public InMemoryTrackingClickRealtimeStore(int suspiciousIpThreshold = 20, int rejectionIpThreshold = 50)
        {
            _suspiciousIpThreshold = suspiciousIpThreshold;
            _rejectionIpThreshold = rejectionIpThreshold;
        }

        public Task<TrackingClickRealtimeResult> RegisterClickAsync(
            TrackingClickRealtimeRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request.ClickedAtUtc < request.TokenIssuedAtUtc || request.ClickedAtUtc > request.TokenExpiresAtUtc)
            {
                return Task.FromResult(new TrackingClickRealtimeResult(
                    IsDuplicate: false,
                    IsRejected: true,
                    IsFlagged: true,
                    DecisionReason: "outside_valid_window",
                    CampaignClickCount: 0,
                    RecipientClickCount: 0));
            }

            string dedupKey = $"{request.CampaignId:D}:{request.RecipientEmail}:{request.TokenNonce}";
            bool alreadySeen = !_dedupKeys.Add(dedupKey);
            if (alreadySeen)
            {
                return Task.FromResult(new TrackingClickRealtimeResult(
                    IsDuplicate: true,
                    IsRejected: false,
                    IsFlagged: true,
                    DecisionReason: "duplicate_nonce",
                    CampaignClickCount: 0,
                    RecipientClickCount: 0));
            }

            _ipCounters.TryGetValue(request.IpAddress, out int currentIpRate);
            int nextIpRate = currentIpRate + 1;
            _ipCounters[request.IpAddress] = nextIpRate;

            if (nextIpRate > _rejectionIpThreshold)
            {
                return Task.FromResult(new TrackingClickRealtimeResult(
                    IsDuplicate: false,
                    IsRejected: true,
                    IsFlagged: true,
                    DecisionReason: "ip_threshold_exceeded",
                    CampaignClickCount: 0,
                    RecipientClickCount: 0));
            }

            bool flagged = nextIpRate > _suspiciousIpThreshold;

            return Task.FromResult(new TrackingClickRealtimeResult(
                IsDuplicate: false,
                IsRejected: false,
                IsFlagged: flagged,
                DecisionReason: flagged ? "ip_rate_suspicious" : null,
                CampaignClickCount: 1,
                RecipientClickCount: 1));
        }
    }

    private sealed class NoOpUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(1);
        }
    }
}
