using System;
using System.Collections.Generic;

using QPhising.Domain.Common;

namespace QPhising.Domain.Templates.ValueObjects;

public sealed class TemplateContent : ValueObject
{
    public const int MaxSubjectLength = 180;
    public const int MaxBodyLength = 200_000;

    public TemplateContent(string subject, string body)
    {
        if (string.IsNullOrWhiteSpace(subject))
        {
            throw new ArgumentException("Template subject is required.", nameof(subject));
        }

        if (string.IsNullOrWhiteSpace(body))
        {
            throw new ArgumentException("Template body is required.", nameof(body));
        }

        var normalizedSubject = subject.Trim();
        var normalizedBody = body.Trim();

        if (normalizedSubject.Length > MaxSubjectLength)
        {
            throw new ArgumentException($"Template subject must be at most {MaxSubjectLength} characters.", nameof(subject));
        }

        if (normalizedBody.Length > MaxBodyLength)
        {
            throw new ArgumentException($"Template body must be at most {MaxBodyLength} characters.", nameof(body));
        }

        Subject = normalizedSubject;
        Body = normalizedBody;
    }

    public string Subject { get; }

    public string Body { get; }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Subject;
        yield return Body;
    }
}
