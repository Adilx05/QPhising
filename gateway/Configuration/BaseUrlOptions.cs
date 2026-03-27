using System.ComponentModel.DataAnnotations;

namespace Gateway.Configuration;

public sealed class BaseUrlOptions
{
    public const string SectionName = "BaseUrls";

    [Required(AllowEmptyStrings = false)]
    [Url]
    public string Gateway { get; init; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    [Url]
    public string Api { get; init; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    [Url]
    public string Frontend { get; init; } = string.Empty;
}
