using QPhising.Application.Common;

namespace QPhising.Application.Common.Abstractions;

public interface ITemplateHtmlSanitizer
{
    Result<string> Sanitize(string htmlContent);
}
