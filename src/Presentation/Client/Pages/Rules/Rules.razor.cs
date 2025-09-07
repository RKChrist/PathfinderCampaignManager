using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using PathfinderCampaignManager.Domain.Entities.Rules;
using PathfinderCampaignManager.Domain.Interfaces;

namespace PathfinderCampaignManager.Presentation.Client.Pages.Rules;

public partial class Rules : ComponentBase
{
    [Parameter] public string? CategoryName { get; set; }
    [Parameter] public string? RuleId { get; set; }

    private bool isLoading = true;
    private string searchTerm = string.Empty;
    private RuleCategory? selectedCategory;
    private RuleRecord? selectedRule;
    private RuleTable? selectedTable;
    private RuleFormula? selectedFormula;
    private Dictionary<RuleCategory, int>? categoryCounts;
    private IEnumerable<RuleRecord>? categoryRules;
    private IEnumerable<RuleRecord>? searchResults;

    protected override async Task OnInitializedAsync()
    {
        await LoadInitialData();
    }

    protected override async Task OnParametersSetAsync()
    {
        await HandleRouteParameters();
    }

    private async Task LoadInitialData()
    {
        isLoading = true;
        StateHasChanged();

        var categoriesResult = await RulesRepository.GetCategoriesAsync();
        if (categoriesResult.IsSuccess)
        {
            categoryCounts = categoriesResult.Value.ToDictionary(c => c, c => 0);
        }

        isLoading = false;
        StateHasChanged();
    }

    private async Task HandleRouteParameters()
    {
        // Handle category parameter
        if (!string.IsNullOrEmpty(CategoryName))
        {
            if (Enum.TryParse<RuleCategory>(CategoryName, true, out var category))
            {
                await SelectCategory(category, updateUrl: false);
            }
        }

        // Handle rule ID parameter
        if (!string.IsNullOrEmpty(RuleId) && !string.IsNullOrEmpty(CategoryName))
        {
            var ruleResult = await RulesRepository.GetRuleAsync(CategoryName, RuleId);
            if (ruleResult.IsSuccess)
            {
                await SelectRule(ruleResult.Value, updateUrl: false);
            }
        }
    }

    private async Task OnSearchInput(ChangeEventArgs e)
    {
        searchTerm = e.Value?.ToString() ?? string.Empty;
        
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            searchResults = null;
            ClearRuleSelection();
            StateHasChanged();
            return;
        }

        if (searchTerm.Length >= 2)
        {
            await PerformSearch();
        }
    }

    private async Task PerformSearch()
    {
        isLoading = true;
        StateHasChanged();

        var searchResult = await RulesRepository.SearchRulesAsync(searchTerm);
        if (searchResult.IsSuccess)
        {
            searchResults = searchResult.Value;
            selectedCategory = null;
            categoryRules = null;
            ClearRuleSelection();
        }

        isLoading = false;
        StateHasChanged();
    }

    private async Task ClearSearch()
    {
        searchTerm = string.Empty;
        searchResults = null;
        ClearRuleSelection();
        StateHasChanged();
    }

    private async Task SelectCategory(RuleCategory category, bool updateUrl = true)
    {
        if (selectedCategory == category)
            return;

        isLoading = true;
        selectedCategory = category;
        searchResults = null;
        searchTerm = string.Empty;
        ClearRuleSelection();
        StateHasChanged();

        var rulesResult = await RulesRepository.GetCategoryRulesAsync(GetCategorySlug(category));
        if (rulesResult.IsSuccess)
        {
            categoryRules = rulesResult.Value;
        }

        if (updateUrl)
        {
            Navigation.NavigateTo($"/rules/{GetCategorySlug(category)}");
        }

        isLoading = false;
        StateHasChanged();
    }

    private async Task SelectRule(RuleRecord rule, bool updateUrl = true)
    {
        selectedRule = rule;
        selectedTable = null;
        selectedFormula = null;

        // Note: Table and formula loading will be handled by the repository if needed
        // For now, we'll leave these as null since the current API doesn't support them

        if (updateUrl)
        {
            var categorySlug = GetCategorySlug(rule.Category);
            Navigation.NavigateTo($"/rules/{categorySlug}/{rule.Id}");
        }

        StateHasChanged();
    }

    private void ClearRuleSelection()
    {
        selectedRule = null;
        selectedTable = null;
        selectedFormula = null;
    }

    private string GetCategorySlug(RuleCategory category)
    {
        return category.ToString().ToLowerInvariant();
    }

    private async Task HandleRuleCardClick(RuleRecord rule)
    {
        await SelectRule(rule);
    }

    private async Task HandleBackClick()
    {
        if (selectedCategory != null)
        {
            Navigation.NavigateTo($"/rules/{GetCategorySlug(selectedCategory.Value)}");
        }
        else if (!string.IsNullOrEmpty(searchTerm))
        {
            // Stay on search results
            Navigation.NavigateTo("/rules");
        }
        else
        {
            Navigation.NavigateTo("/rules");
        }

        ClearRuleSelection();
        StateHasChanged();
    }
}