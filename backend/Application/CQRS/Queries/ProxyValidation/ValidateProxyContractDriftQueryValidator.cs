using FluentValidation;

namespace QPhising.Application.CQRS.Queries.ProxyValidation;

public sealed class ValidateProxyContractDriftQueryValidator : AbstractValidator<ValidateProxyContractDriftQuery>
{
    public ValidateProxyContractDriftQueryValidator()
    {
        RuleFor(query => query.SwaggerContractPath)
            .NotEmpty()
            .WithMessage("Swagger contract path is required for drift validation.");

        RuleFor(query => query.ProxyGenerationStampPath)
            .NotEmpty()
            .WithMessage("Proxy generation stamp path is required for drift validation.");
    }
}
