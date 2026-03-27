using QPhising.Domain.Campaigns.Events;
using QPhising.Domain.Campaigns.Exceptions;

namespace QPhising.Domain.Campaigns;

public sealed class Campaign
{
    private const int MaxNameLength = 200;

    private static readonly IReadOnlyDictionary<CampaignStatus, CampaignStatus[]> AllowedStatusTransitions =
        new Dictionary<CampaignStatus, CampaignStatus[]>
        {
            [CampaignStatus.Draft] = [CampaignStatus.Scheduled],
            [CampaignStatus.Scheduled] = [CampaignStatus.Active],
            [CampaignStatus.Active] = [CampaignStatus.Ended, CampaignStatus.Archived],
            [CampaignStatus.Ended] = [],
            [CampaignStatus.Archived] = []
        };

    private readonly List<CampaignStatusChangedDomainEvent> _statusEvents = [];

    private Campaign(
        Guid id,
        string name,
        TemplateType templateType,
        string htmlContent,
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        CampaignStatus status)
    {
        Id = id;
        Name = name;
        TemplateType = templateType;
        HtmlContent = htmlContent;
        StartDate = startDate;
        EndDate = endDate;
        Status = status;
    }

    public Guid Id { get; }

    public string Name { get; private set; }

    public TemplateType TemplateType { get; private set; }

    public string HtmlContent { get; private set; }

    public DateTimeOffset StartDate { get; private set; }

    public DateTimeOffset EndDate { get; private set; }

    public CampaignStatus Status { get; private set; }

    public IReadOnlyCollection<CampaignStatusChangedDomainEvent> StatusEvents => _statusEvents.AsReadOnly();

    public static Campaign Create(
        string name,
        TemplateType templateType,
        string htmlContent,
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        Guid? id = null)
    {
        ValidateName(name);
        ValidateHtmlContent(htmlContent);
        ValidateDateRange(startDate, endDate);

        return new Campaign(
            id ?? Guid.NewGuid(),
            name.Trim(),
            templateType,
            htmlContent.Trim(),
            startDate,
            endDate,
            CampaignStatus.Draft);
    }

    public void UpdateDetails(
        string name,
        TemplateType templateType,
        string htmlContent,
        DateTimeOffset startDate,
        DateTimeOffset endDate)
    {
        ValidateName(name);
        ValidateHtmlContent(htmlContent);
        ValidateDateRange(startDate, endDate);

        Name = name.Trim();
        TemplateType = templateType;
        HtmlContent = htmlContent.Trim();
        StartDate = startDate;
        EndDate = endDate;
    }

    public CampaignStatusChangedDomainEvent ChangeStatus(CampaignStatus newStatus, DateTimeOffset? occurredAt = null)
    {
        if (Status == newStatus)
        {
            throw new InvalidCampaignStatusTransitionException(Status, newStatus);
        }

        if (!AllowedStatusTransitions.TryGetValue(Status, out CampaignStatus[]? allowedStatuses) ||
            !allowedStatuses.Contains(newStatus))
        {
            throw new InvalidCampaignStatusTransitionException(Status, newStatus);
        }

        CampaignStatus previousStatus = Status;
        Status = newStatus;

        CampaignStatusChangedDomainEvent domainEvent = new(
            Id,
            previousStatus,
            newStatus,
            occurredAt ?? DateTimeOffset.UtcNow);

        _statusEvents.Add(domainEvent);

        return domainEvent;
    }

    public bool IsExpired(DateTimeOffset now) => EndDate < now;

    public void ClearStatusEvents() => _statusEvents.Clear();

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new CampaignValidationException("Campaign name is required.");
        }

        if (name.Trim().Length > MaxNameLength)
        {
            throw new CampaignValidationException($"Campaign name must be {MaxNameLength} characters or fewer.");
        }
    }

    private static void ValidateHtmlContent(string htmlContent)
    {
        if (string.IsNullOrWhiteSpace(htmlContent))
        {
            throw new CampaignValidationException("Campaign HTML content is required.");
        }
    }

    private static void ValidateDateRange(DateTimeOffset startDate, DateTimeOffset endDate)
    {
        if (startDate > endDate)
        {
            throw new CampaignValidationException("Campaign start date must be less than or equal to end date.");
        }
    }
}
