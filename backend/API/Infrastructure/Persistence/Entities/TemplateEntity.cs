namespace QPhising.Api.Infrastructure.Persistence.Entities;

public sealed class TemplateEntity : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string HtmlContent { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Tags { get; set; } = string.Empty;

    public int LifecycleState { get; set; }

    public int Version { get; set; }

}
