using System;

namespace QPhising.Domain.Persistence.Models;

public sealed class AggregateFieldMapping
{
    public AggregateFieldMapping(string domainProperty, string storageField, bool isRequired, bool containsSecret)
    {
        DomainProperty = string.IsNullOrWhiteSpace(domainProperty)
            ? throw new ArgumentException("Domain property is required.", nameof(domainProperty))
            : domainProperty.Trim();

        StorageField = string.IsNullOrWhiteSpace(storageField)
            ? throw new ArgumentException("Storage field is required.", nameof(storageField))
            : storageField.Trim();

        IsRequired = isRequired;
        ContainsSecret = containsSecret;
    }

    public string DomainProperty { get; }

    public string StorageField { get; }

    public bool IsRequired { get; }

    public bool ContainsSecret { get; }
}
