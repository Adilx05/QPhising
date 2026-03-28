using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using QPhising.Application.Common.Abstractions;

namespace QPhising.Application.Common;

public sealed partial class TemplateHtmlSanitizer : ITemplateHtmlSanitizer
{
    private static readonly HashSet<string> AllowedTags =
    [
        "a", "article", "aside", "b", "blockquote", "br", "caption", "code", "dd", "div", "dl", "dt",
        "em", "figcaption", "figure", "footer", "h1", "h2", "h3", "h4", "h5", "h6", "header", "hr", "i",
        "img", "li", "main", "ol", "p", "pre", "section", "small", "span", "strong", "sub", "sup",
        "table", "tbody", "td", "th", "thead", "tr", "u", "ul"
    ];

    private static readonly HashSet<string> AllowedGlobalAttributes = ["class", "id", "title", "aria-label", "role"];

    private static readonly Dictionary<string, HashSet<string>> AllowedTagAttributes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["a"] = ["href", "target", "rel"],
        ["img"] = ["src", "alt", "width", "height", "loading"],
        ["th"] = ["scope", "colspan", "rowspan"],
        ["td"] = ["colspan", "rowspan"]
    };

    public Result<string> Sanitize(string htmlContent)
    {
        if (string.IsNullOrWhiteSpace(htmlContent))
        {
            return Result<string>.Failure("Template HTML content is required.");
        }

        string normalized = htmlContent.Trim();

        string withoutComments = HtmlCommentPattern().Replace(normalized, string.Empty);
        string sanitized = HtmlTagPattern().Replace(withoutComments, SanitizeTagMatch);
        return Result<string>.Success(sanitized.Trim());
    }

    private static string SanitizeTagMatch(Match match)
    {
        string isClosingTagValue = match.Groups["closing"].Value;
        bool isClosingTag = string.Equals(isClosingTagValue, "/", StringComparison.Ordinal);

        string tagName = match.Groups["tag"].Value.ToLowerInvariant();
        if (!AllowedTags.Contains(tagName))
        {
            return string.Empty;
        }

        if (isClosingTag)
        {
            return $"</{tagName}>";
        }

        string rawAttributes = match.Groups["attrs"].Value;
        var attributesBuilder = new StringBuilder();

        foreach (Match attributeMatch in AttributePattern().Matches(rawAttributes))
        {
            string attributeName = attributeMatch.Groups["name"].Value.ToLowerInvariant();
            if (!IsAllowedAttribute(tagName, attributeName))
            {
                continue;
            }

            string attributeValue = ExtractAttributeValue(attributeMatch);
            if (attributeName is "href" or "src" && !IsSafeUriValue(attributeValue))
            {
                continue;
            }

            attributesBuilder
                .Append(' ')
                .Append(attributeName)
                .Append("=\"")
                .Append(WebUtility.HtmlEncode(attributeValue))
                .Append('"');
        }

        bool isSelfClosing = string.Equals(match.Groups["self"].Value, "/", StringComparison.Ordinal);
        return isSelfClosing
            ? $"<{tagName}{attributesBuilder} />"
            : $"<{tagName}{attributesBuilder}>";
    }

    private static bool IsAllowedAttribute(string tagName, string attributeName)
    {
        if (AllowedGlobalAttributes.Contains(attributeName))
        {
            return true;
        }

        return AllowedTagAttributes.TryGetValue(tagName, out HashSet<string>? allowedAttributes) &&
               allowedAttributes.Contains(attributeName);
    }

    private static string ExtractAttributeValue(Match attributeMatch)
    {
        if (attributeMatch.Groups["value1"].Success)
        {
            return attributeMatch.Groups["value1"].Value.Trim();
        }

        if (attributeMatch.Groups["value2"].Success)
        {
            return attributeMatch.Groups["value2"].Value.Trim();
        }

        return attributeMatch.Groups["value3"].Value.Trim();
    }

    private static bool IsSafeUriValue(string uriValue)
    {
        if (string.IsNullOrWhiteSpace(uriValue))
        {
            return false;
        }

        if (uriValue.StartsWith('#') || uriValue.StartsWith('/'))
        {
            return true;
        }

        return Uri.TryCreate(uriValue, UriKind.Absolute, out Uri? uri) &&
               (uri.Scheme == Uri.UriSchemeHttp ||
                uri.Scheme == Uri.UriSchemeHttps ||
                uri.Scheme == Uri.UriSchemeMailto ||
                uri.Scheme == "tel");
    }

    [GeneratedRegex("<!--.*?-->", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant)]
    private static partial Regex HtmlCommentPattern();

    [GeneratedRegex("<(?<closing>/)?(?<tag>[a-zA-Z][a-zA-Z0-9]*)(?<attrs>[^<>]*?)(?<self>/?)>", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex HtmlTagPattern();

    [GeneratedRegex("(?<name>[a-zA-Z_:][-a-zA-Z0-9_:.]*)\\s*=\\s*(?:\"(?<value1>[^\"]*)\"|'(?<value2>[^']*)'|(?<value3>[^\\s\"'`=<>]+))", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex AttributePattern();
}
