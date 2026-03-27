using System.ComponentModel.DataAnnotations;

namespace QPhising.Infrastructure.Exports;

public sealed class ExportStorageOptions
{
    public const string SectionName = "ExportStorage";

    [Required]
    public string BasePath { get; set; } = string.Empty;

    [Range(1, 365)]
    public int FileTtlDays { get; set; } = 7;
}
