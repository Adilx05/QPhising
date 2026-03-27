namespace QPhising.Application.Common;

public sealed record Result<T>(bool IsSuccess, T? Value, IReadOnlyCollection<string> Errors)
{
    public static Result<T> Success(T value) => new(true, value, Array.Empty<string>());
    public static Result<T> Failure(params string[] errors) => new(false, default, errors);
}
