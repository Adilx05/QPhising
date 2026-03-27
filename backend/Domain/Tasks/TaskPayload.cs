using QPhising.Domain.Tasks.Exceptions;

namespace QPhising.Domain.Tasks;

public sealed class TaskPayload
{
    private readonly Dictionary<string, string> _values;

    private TaskPayload(Dictionary<string, string> values)
    {
        _values = values;
    }

    public IReadOnlyDictionary<string, string> Values => _values;

    public static TaskPayload Create(IReadOnlyDictionary<string, string> values)
    {
        if (values.Count == 0)
        {
            throw new TaskValidationException("Task payload must include at least one key/value pair.");
        }

        Dictionary<string, string> normalized = [];

        foreach ((string key, string value) in values)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new TaskValidationException("Task payload keys must be non-empty.");
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new TaskValidationException($"Task payload value for key '{key}' is required.");
            }

            normalized[key.Trim()] = value.Trim();
        }

        return new TaskPayload(normalized);
    }

    public bool ContainsKey(string key) => _values.ContainsKey(key);

    public string GetRequired(string key)
    {
        if (!_values.TryGetValue(key, out string? value))
        {
            throw new TaskValidationException($"Task payload key '{key}' is required.");
        }

        return value;
    }
}
