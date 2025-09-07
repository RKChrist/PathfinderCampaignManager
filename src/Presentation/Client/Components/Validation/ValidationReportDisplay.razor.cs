using Microsoft.AspNetCore.Components;
using PathfinderCampaignManager.Domain.Validation;
using PathfinderCampaignManager.Domain.Entities.Pathfinder;
using PathfinderCampaignManager.Domain.Interfaces;

namespace PathfinderCampaignManager.Presentation.Client.Components.Validation;

public partial class ValidationReportDisplay : ComponentBase
{
    [Parameter] public ValidationReport? Report { get; set; }
    [Parameter] public bool Compact { get; set; } = false;

    private string GetContainerClass()
    {
        var classes = new List<string>();
        
        if (Compact)
            classes.Add("compact");
            
        if (Report != null)
        {
            if (Report.IsValid)
                classes.Add("valid");
            else if (Report.HasCriticalIssues)
                classes.Add("invalid");
            else
                classes.Add("warnings");
        }
        
        return string.Join(" ", classes);
    }

    private string GetStatusIcon()
    {
        if (Report == null)
            return "fa-question-circle";
            
        return Report.IsValid switch
        {
            true => "fa-check-circle",
            false when Report.HasCriticalIssues => "fa-times-circle",
            false => "fa-exclamation-triangle"
        };
    }

    private string GetSeverityClass(ValidationSeverity severity)
    {
        return severity switch
        {
            ValidationSeverity.Error => "error",
            ValidationSeverity.Warning => "warning",
            _ => ""
        };
    }

    private string GetSeverityIcon(ValidationSeverity severity)
    {
        return severity switch
        {
            ValidationSeverity.Error => "fa-times-circle",
            ValidationSeverity.Warning => "fa-exclamation-triangle",
            _ => "fa-info-circle"
        };
    }
}