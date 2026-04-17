using System.ComponentModel.DataAnnotations;

namespace QPhising.Api.Contracts.ProxyValidation;

public sealed class AssertProxyContractSyncRequest
{
    [Required]
    public string SwaggerContractPath { get; init; } = string.Empty;

    [Required]
    public string ProxyGenerationStampPath { get; init; } = string.Empty;
}
