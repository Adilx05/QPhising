using QPhising.Application.Contracts.Responses.ProxyValidation;

namespace QPhising.Application.CQRS.Commands.ProxyValidation;

public sealed class ProxyContractDriftException : Exception
{
    public ProxyContractDriftException(ProxyDriftValidationResult validationResult)
        : base(validationResult.Message)
    {
        ValidationResult = validationResult;
    }

    public ProxyDriftValidationResult ValidationResult { get; }
}
