using QPhising.Application.Common;

namespace QPhising.Application.Common.Abstractions;

public interface ITemplateVariableSubstitutionEngine
{
    Result<string> Render(
        string htmlContent,
        IReadOnlyCollection<string> allowedVariables,
        IReadOnlyDictionary<string, string> variableValues);
}
