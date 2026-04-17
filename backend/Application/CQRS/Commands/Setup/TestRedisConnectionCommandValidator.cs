using FluentValidation;

namespace QPhising.Application.CQRS.Commands.Setup;

public sealed class TestRedisConnectionCommandValidator : AbstractValidator<TestRedisConnectionCommand>
{
    public TestRedisConnectionCommandValidator()
    {
        RuleFor(command => command.ConnectionString)
            .NotEmpty()
            .WithMessage("Redis connection string is required.");
    }
}
