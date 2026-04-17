namespace QPhising.Application.Contracts.Responses.ProxyValidation;

public enum ProxyDriftValidationStatus
{
    InSync = 0,
    SwaggerContractChanged = 1,
    ProxyMissing = 2,
    ValidationError = 3
}
