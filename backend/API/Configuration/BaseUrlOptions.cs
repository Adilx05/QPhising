using System.ComponentModel.DataAnnotations;

namespace QPhising.API.Configuration;

public sealed class BaseUrlOptions
{
    public const string SectionName = "BaseUrls";

    [Required(AllowEmptyStrings = false)]
    [Url]
    public string Gateway { get; init; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    [Url]
    public string Frontend { get; init; } = string.Empty;
}
