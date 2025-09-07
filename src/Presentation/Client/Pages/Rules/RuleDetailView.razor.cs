using Microsoft.AspNetCore.Components;
using PathfinderCampaignManager.Domain.Entities.Rules;
using System.Text.RegularExpressions;

namespace PathfinderCampaignManager.Presentation.Client.Pages.Rules;

public partial class RuleDetailView : ComponentBase
{
    [Parameter] public RuleRecord Rule { get; set; } = default!;
    [Parameter] public RuleTable? Table { get; set; }
    [Parameter] public RuleFormula? Formula { get; set; }
    [Parameter] public EventCallback OnBackClick { get; set; }

    private async Task HandleBackClick()
    {
        if (OnBackClick.HasDelegate)
        {
            await OnBackClick.InvokeAsync();
        }
    }

    private string GetContentTypeCssClass()
    {
        return Rule.ContentType.ToString().ToLowerInvariant();
    }

    private string ConvertMarkdownToHtml(string markdown)
    {
        if (string.IsNullOrEmpty(markdown))
            return string.Empty;

        var html = markdown;

        // Convert headers
        html = Regex.Replace(html, @"^### (.+)$", "<h4>$1</h4>", RegexOptions.Multiline);
        html = Regex.Replace(html, @"^## (.+)$", "<h3>$1</h3>", RegexOptions.Multiline);
        html = Regex.Replace(html, @"^# (.+)$", "<h2>$1</h2>", RegexOptions.Multiline);

        // Convert bold and italic
        html = Regex.Replace(html, @"\*\*(.+?)\*\*", "<strong>$1</strong>");
        html = Regex.Replace(html, @"\*(.+?)\*", "<em>$1</em>");

        // Convert code blocks
        html = Regex.Replace(html, @"`([^`]+)`", "<code>$1</code>");

        // Convert line breaks
        html = Regex.Replace(html, @"\n\n", "</p><p>");
        html = Regex.Replace(html, @"\n", "<br>");

        // Wrap in paragraph if not already wrapped
        if (!html.StartsWith("<h") && !html.StartsWith("<p") && !html.StartsWith("<ul") && !html.StartsWith("<ol"))
        {
            html = $"<p>{html}</p>";
        }

        // Convert unordered lists
        html = Regex.Replace(html, @"^- (.+)$", "<li>$1</li>", RegexOptions.Multiline);
        html = Regex.Replace(html, @"(<li>.*?</li>)", "<ul>$1</ul>", RegexOptions.Singleline);

        // Convert ordered lists
        html = Regex.Replace(html, @"^\d+\. (.+)$", "<li>$1</li>", RegexOptions.Multiline);
        html = Regex.Replace(html, @"(<li>.*?</li>)", "<ol>$1</ol>", RegexOptions.Singleline);

        return html;
    }
}