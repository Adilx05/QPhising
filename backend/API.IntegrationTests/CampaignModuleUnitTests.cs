using AutoMapper;
using QPhising.Application.Common;
using QPhising.Application.Common.Abstractions;
using QPhising.Application.Features.Analytics.GetDashboardKpis;
using QPhising.Application.Features.Campaigns.ActivateCampaign;
using QPhising.Application.Features.Campaigns.CreateCampaign;
using QPhising.Application.Features.Campaigns.ScheduleCampaign;
using QPhising.Application.Features.Campaigns.UpdateCampaign;
using QPhising.Domain.Abstractions;
using QPhising.Domain.Campaigns;
using QPhising.Domain.Campaigns.Exceptions;
using Xunit;

namespace QPhising.API.IntegrationTests;

public sealed class CampaignModuleUnitTests
{
    private readonly IMapper _mapper;

    public CampaignModuleUnitTests()
    {
        MapperConfiguration configuration = new(cfg =>
        {
            cfg.AddProfile<CreateCampaignMappingProfile>();
            cfg.AddProfile<UpdateCampaignMappingProfile>();
            cfg.AddProfile<ScheduleCampaignMappingProfile>();
            cfg.AddProfile<ActivateCampaignMappingProfile>();
        });

        _mapper = configuration.CreateMapper();
    }

    [Fact]
    public void Campaign_Create_Should_Reject_Invalid_Date_Range()
    {
        var now = DateTimeOffset.UtcNow;

        CampaignValidationException exception = Assert.Throws<CampaignValidationException>(() => Campaign.Create(
            "Invalid",
            TemplateType.Email,
            "<h1>Html</h1>",
            now.AddDays(2),
            now.AddDays(1)));

        Assert.Equal("Campaign start date must be less than or equal to end date.", exception.Message);
    }

    [Fact]
    public void Campaign_ChangeStatus_Should_Reject_Draft_To_Active_Transition()
    {
        var now = DateTimeOffset.UtcNow;
        var campaign = Campaign.Create(
            "Quarterly Simulation",
            TemplateType.Email,
            "<h1>Training</h1>",
            now.AddDays(1),
            now.AddDays(5));

        Assert.Throws<InvalidCampaignStatusTransitionException>(() => campaign.ChangeStatus(CampaignStatus.Active));
    }

    [Fact]
    public async Task CreateCampaignCommandHandler_Should_Persist_And_Return_Draft_Response()
    {
        var now = DateTimeOffset.UtcNow;
        var repository = new InMemoryCampaignRepository();
        var unitOfWork = new RecordingUnitOfWork();
        var analyticsCache = new RecordingAnalyticsDashboardCache();
        var notifier = new RecordingAnalyticsRealtimeNotifier();
        var handler = new CreateCampaignCommandHandler(repository, unitOfWork, _mapper, analyticsCache, notifier);

        var result = await handler.Handle(new CreateCampaignCommand(
            "Campaign A",
            TemplateType.Email,
            "<h1>Body</h1>",
            now.AddDays(1),
            now.AddDays(10)), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(CampaignStatus.Draft, result.Value!.Status);
        Assert.Equal(1, repository.CampaignCount);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
        Assert.Equal(1, analyticsCache.InvalidationCount);
        Assert.Equal(1, notifier.PublishCount);
    }

    [Fact]
    public async Task UpdateCampaignCommandHandler_Should_Return_Failure_When_Campaign_Not_Found()
    {
        var now = DateTimeOffset.UtcNow;
        var repository = new InMemoryCampaignRepository();
        var unitOfWork = new RecordingUnitOfWork();
        var handler = new UpdateCampaignCommandHandler(repository, unitOfWork, _mapper, new RecordingAnalyticsDashboardCache(), new RecordingAnalyticsRealtimeNotifier());

        var result = await handler.Handle(new UpdateCampaignCommand(
            Guid.NewGuid(),
            "Campaign B",
            TemplateType.LandingPage,
            "<h1>Updated</h1>",
            now.AddDays(2),
            now.AddDays(11)), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("was not found", result.Errors.Single());
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task ScheduleCampaignCommandHandler_Should_Return_Failure_When_Scheduling_Window_Is_Invalid()
    {
        var now = DateTimeOffset.UtcNow;
        var campaign = Campaign.Create(
            "Late Scheduling",
            TemplateType.Email,
            "<h1>Body</h1>",
            now.AddHours(-1),
            now.AddDays(1));

        var repository = new InMemoryCampaignRepository(campaign);
        var unitOfWork = new RecordingUnitOfWork();
        var handler = new ScheduleCampaignCommandHandler(repository, unitOfWork, _mapper, new RecordingAnalyticsDashboardCache(), new RecordingAnalyticsRealtimeNotifier());

        var result = await handler.Handle(new ScheduleCampaignCommand(campaign.Id), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("scheduled before its start date", result.Errors.Single());
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task ActivateCampaignCommandHandler_Should_Activate_Scheduled_Campaign()
    {
        var now = DateTimeOffset.UtcNow;
        var campaign = Campaign.Create(
            "Activation Flow",
            TemplateType.Email,
            "<h1>Body</h1>",
            now.AddMinutes(-2),
            now.AddDays(2));
        campaign.Schedule(now.AddMinutes(-3));

        var repository = new InMemoryCampaignRepository(campaign);
        var unitOfWork = new RecordingUnitOfWork();
        var analyticsCache = new RecordingAnalyticsDashboardCache();
        var notifier = new RecordingAnalyticsRealtimeNotifier();
        var handler = new ActivateCampaignCommandHandler(repository, unitOfWork, _mapper, analyticsCache, notifier);

        var result = await handler.Handle(new ActivateCampaignCommand(campaign.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(CampaignStatus.Active, result.Value!.Status);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
        Assert.Equal(1, analyticsCache.InvalidationCount);
        Assert.Equal(1, notifier.PublishCount);
    }

    private sealed class RecordingAnalyticsRealtimeNotifier : IAnalyticsRealtimeNotifier
    {
        public int PublishCount { get; private set; }

        public Task PublishDashboardUpdatedAsync(AnalyticsDashboardUpdatedEvent updateEvent, CancellationToken cancellationToken = default)
        {
            PublishCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class RecordingAnalyticsDashboardCache : IAnalyticsDashboardCache
    {
        public int InvalidationCount { get; private set; }

        public Task<DashboardKpisResponse?> GetAsync(GetDashboardKpisQuery query, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<DashboardKpisResponse?>(null);
        }

        public Task SetAsync(GetDashboardKpisQuery query, DashboardKpisResponse response, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task InvalidateAsync(CancellationToken cancellationToken = default)
        {
            InvalidationCount++;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public void CreateCampaignCommandValidator_Should_Reject_StartDate_After_EndDate()
    {
        var now = DateTimeOffset.UtcNow;
        var validator = new CreateCampaignCommandValidator();

        var validation = validator.Validate(new CreateCampaignCommand(
            "Validator",
            TemplateType.Email,
            "<h1>Body</h1>",
            now.AddDays(5),
            now.AddDays(1)));

        Assert.False(validation.IsValid);
        Assert.Contains(validation.Errors, error =>
            error.PropertyName == nameof(CreateCampaignCommand.StartDate) &&
            error.ErrorMessage == "Campaign start date must be less than or equal to end date.");
    }

    private sealed class RecordingUnitOfWork : IUnitOfWork
    {
        public int SaveChangesCallCount { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesCallCount++;
            return Task.FromResult(1);
        }
    }

    private sealed class InMemoryCampaignRepository : ICampaignRepository
    {
        private readonly Dictionary<Guid, Campaign> _campaigns;

        public InMemoryCampaignRepository(params Campaign[] campaigns)
        {
            _campaigns = campaigns.ToDictionary(campaign => campaign.Id, campaign => campaign);
        }

        public int CampaignCount => _campaigns.Count;

        public Task<Campaign?> GetByIdAsync(Guid campaignId, CancellationToken cancellationToken = default)
        {
            _campaigns.TryGetValue(campaignId, out Campaign? campaign);
            return Task.FromResult(campaign);
        }

        public Task<IReadOnlyCollection<Campaign>> ListAsync(CampaignReadCriteria criteria, CancellationToken cancellationToken = default)
        {
            IReadOnlyCollection<Campaign> campaigns = _campaigns.Values.ToArray();
            return Task.FromResult(campaigns);
        }

        public Task<IReadOnlyCollection<Campaign>> ListOverlappingWindowAsync(
            DateTimeOffset windowStart,
            DateTimeOffset windowEnd,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyCollection<Campaign> campaigns = _campaigns.Values
                .Where(campaign => campaign.StartDate <= windowEnd && campaign.EndDate >= windowStart)
                .ToArray();

            return Task.FromResult(campaigns);
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
}
