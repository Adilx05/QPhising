using System;
using System.Collections.Generic;

using QPhising.Domain.Common;

namespace QPhising.Domain.Templates.ValueObjects;

public sealed class TemplateContent : ValueObject
{
    public const int MaxHtmlLength = 200_000;

    public TemplateContent(string htmlContent)
    {
        if (string.IsNullOrWhiteSpace(htmlContent))
        {
            throw new ArgumentException("Template HTML content is required.", nameof(htmlContent));
        }

        var normalizedHtml = htmlContent.Trim();

        if (normalizedHtml.Length > MaxHtmlLength)
        {
            throw new ArgumentException($"Template HTML content must be at most {MaxHtmlLength} characters.", nameof(htmlContent));
        }

        HtmlContent = normalizedHtml;
    }

    public string HtmlContent { get; }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return HtmlContent;
    }
}
