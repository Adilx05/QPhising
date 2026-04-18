using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using QPhising.Domain.Common;

namespace QPhising.Domain.Templates.ValueObjects;

public sealed class TemplateMetadata : ValueObject
{
    public const int MaxDescriptionLength = 500;
    public const int MaxTagLength = 32;
    public const int MaxTagCount = 10;

    private readonly List<string> _tags;

    public TemplateMetadata(string? description, IEnumerable<string>? tags)
    {
        Description = NormalizeDescription(description);
        _tags = NormalizeTags(tags);
    }

    public string? Description { get; }

    public IReadOnlyCollection<string> Tags => new ReadOnlyCollection<string>(_tags);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Description;

        foreach (var tag in _tags)
        {
            yield return tag;
        }
    }

    private static string? NormalizeDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return null;
        }

        var normalized = description.Trim();

        if (normalized.Length > MaxDescriptionLength)
        {
            throw new ArgumentException($"Template description must be at most {MaxDescriptionLength} characters.", nameof(description));
        }

        return normalized;
    }

    private static List<string> NormalizeTags(IEnumerable<string>? tags)
    {
        if (tags is null)
        {
            return [];
        }

        var normalized = tags
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .Select(tag => tag.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (normalized.Count > MaxTagCount)
        {
            throw new ArgumentException($"Template metadata supports at most {MaxTagCount} tags.", nameof(tags));
        }

        if (normalized.Any(tag => tag.Length > MaxTagLength))
        {
            throw new ArgumentException($"Each template tag must be at most {MaxTagLength} characters.", nameof(tags));
        }

        return normalized;
    }
}
