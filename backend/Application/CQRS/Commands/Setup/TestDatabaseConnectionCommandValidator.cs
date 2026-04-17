using FluentValidation;

namespace QPhising.Application.CQRS.Commands.Setup;

public sealed class TestDatabaseConnectionCommandValidator : AbstractValidator<TestDatabaseConnectionCommand>
{
    public TestDatabaseConnectionCommandValidator()
    {
        RuleFor(command => command.ConnectionString)
            .NotEmpty()
            .WithMessage("Database connection string is required.");
    }
}
