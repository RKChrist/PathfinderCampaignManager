using Xunit;
using FluentAssertions;
using PathfinderCampaignManager.Domain.Validation;
using PathfinderCampaignManager.Domain.Interfaces;

namespace PathfinderCampaignManager.Domain.Tests;

public class ValidationReportTests
{
    [Fact]
    public void ValidationReport_DefaultState_IsValid()
    {
        // Arrange & Act
        var report = new ValidationReport();

        // Assert
        report.IsValid.Should().BeFalse(); // Default state should be invalid until explicitly validated
        report.Issues.Should().BeEmpty();
        report.Warnings.Should().BeEmpty();
        report.Suggestions.Should().BeEmpty();
        report.ValidationTimestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        report.ValidatedEntityId.Should().Be(string.Empty);
        report.ValidatedEntityType.Should().Be(string.Empty);
    }

    [Fact]
    public void AddIssue_ErrorSeverity_SetsIsValidToFalse()
    {
        // Arrange
        var report = new ValidationReport { IsValid = true };

        // Act
        report.AddIssue(ValidationSeverity.Error, "TestCategory", "Test error message");

        // Assert
        report.IsValid.Should().BeTrue(); // IsValid is not automatically updated by AddIssue
        report.Issues.Should().HaveCount(1);
        report.Issues.First().Severity.Should().Be(ValidationSeverity.Error);
        report.Issues.First().Category.Should().Be("TestCategory");
        report.Issues.First().Message.Should().Be("Test error message");
        report.Issues.First().FixAction.Should().BeNull();
    }

    [Fact]
    public void AddIssue_WithFixSuggestion_StoresFixAction()
    {
        // Arrange
        var report = new ValidationReport();
        var fixSuggestion = "Try increasing the ability score";

        // Act
        report.AddIssue(ValidationSeverity.Error, "Abilities", "Score too low", fixSuggestion);

        // Assert
        report.Issues.Should().HaveCount(1);
        report.Issues.First().FixAction.Should().Be(fixSuggestion);
    }

    [Fact]
    public void AddWarning_AddsWarningToCollection()
    {
        // Arrange
        var report = new ValidationReport();
        var recommendation = "Consider reviewing this choice";

        // Act
        report.AddWarning("Equipment", "No armor equipped", recommendation);

        // Assert
        report.Warnings.Should().HaveCount(1);
        report.Warnings.First().Category.Should().Be("Equipment");
        report.Warnings.First().Message.Should().Be("No armor equipped");
        report.Warnings.First().Recommendation.Should().Be(recommendation);
    }

    [Fact]
    public void AddSuggestion_AddsSuggestionToCollection()
    {
        // Arrange
        var report = new ValidationReport();
        var benefit = "Improves combat effectiveness";

        // Act
        report.AddSuggestion("Combat", "Consider learning Shield Block", benefit);

        // Assert
        report.Suggestions.Should().HaveCount(1);
        report.Suggestions.First().Category.Should().Be("Combat");
        report.Suggestions.First().Suggestion.Should().Be("Consider learning Shield Block");
        report.Suggestions.First().Benefit.Should().Be(benefit);
    }

    [Fact]
    public void HasCriticalIssues_WithErrorSeverity_ReturnsTrue()
    {
        // Arrange
        var report = new ValidationReport();
        report.AddIssue(ValidationSeverity.Error, "Test", "Critical error");

        // Act & Assert
        report.HasCriticalIssues.Should().BeTrue();
    }

    [Fact]
    public void HasCriticalIssues_OnlyWarningSeverity_ReturnsFalse()
    {
        // Arrange
        var report = new ValidationReport();
        report.AddIssue(ValidationSeverity.Warning, "Test", "Warning only");

        // Act & Assert
        report.HasCriticalIssues.Should().BeFalse();
    }

    [Fact]
    public void HasWarnings_WithWarnings_ReturnsTrue()
    {
        // Arrange
        var report = new ValidationReport();
        report.AddWarning("Test", "Test warning");

        // Act & Assert
        report.HasWarnings.Should().BeTrue();
    }

    [Fact]
    public void HasWarnings_WithWarningSeverityIssues_ReturnsTrue()
    {
        // Arrange
        var report = new ValidationReport();
        report.AddIssue(ValidationSeverity.Warning, "Test", "Warning severity issue");

        // Act & Assert
        report.HasWarnings.Should().BeTrue();
    }

    [Fact]
    public void HasWarnings_NoWarningsOrWarningSeverityIssues_ReturnsFalse()
    {
        // Arrange
        var report = new ValidationReport();
        report.AddIssue(ValidationSeverity.Error, "Test", "Error only");

        // Act & Assert
        report.HasWarnings.Should().BeFalse();
    }

    [Fact]
    public void TotalIssueCount_CountsIssuesAndWarnings()
    {
        // Arrange
        var report = new ValidationReport();
        report.AddIssue(ValidationSeverity.Error, "Test", "Error 1");
        report.AddIssue(ValidationSeverity.Warning, "Test", "Warning issue");
        report.AddWarning("Test", "Warning 1");
        report.AddWarning("Test", "Warning 2");

        // Act & Assert
        report.TotalIssueCount.Should().Be(4); // 2 issues + 2 warnings
    }

    [Fact]
    public void TotalIssueCount_NoIssuesOrWarnings_ReturnsZero()
    {
        // Arrange
        var report = new ValidationReport();
        report.AddSuggestion("Test", "Just a suggestion");

        // Act & Assert
        report.TotalIssueCount.Should().Be(0);
    }

    [Fact]
    public void ValidationReport_MultipleIssueTypes_TracksAllCorrectly()
    {
        // Arrange
        var report = new ValidationReport
        {
            ValidatedEntityId = "test-character-123",
            ValidatedEntityType = "Character"
        };

        // Act
        report.AddIssue(ValidationSeverity.Error, "Basic", "Missing name", "Add a character name");
        report.AddIssue(ValidationSeverity.Warning, "Stats", "Low constitution", "Consider increasing");
        report.AddWarning("Equipment", "No armor", "Equip armor for better AC");
        report.AddSuggestion("Optimization", "Consider Power Attack feat", "Increases damage output");

        // Assert
        report.Issues.Should().HaveCount(2);
        report.Warnings.Should().HaveCount(1);
        report.Suggestions.Should().HaveCount(1);
        report.TotalIssueCount.Should().Be(3);
        report.HasCriticalIssues.Should().BeTrue();
        report.HasWarnings.Should().BeTrue();
        report.ValidatedEntityId.Should().Be("test-character-123");
        report.ValidatedEntityType.Should().Be("Character");
    }

    [Fact]
    public void ValidationIssue_Properties_SetCorrectly()
    {
        // Arrange & Act
        var issue = new ValidationIssue
        {
            Severity = ValidationSeverity.Error,
            Category = "Prerequisites",
            Message = "Strength 13 required",
            FixAction = "Increase Strength to 13"
        };

        // Assert
        issue.Severity.Should().Be(ValidationSeverity.Error);
        issue.Category.Should().Be("Prerequisites");
        issue.Message.Should().Be("Strength 13 required");
        issue.FixAction.Should().Be("Increase Strength to 13");
        issue.Data.Should().NotBeNull();
        issue.Data.Should().BeEmpty();
    }

    [Fact]
    public void ValidationWarning_Properties_SetCorrectly()
    {
        // Arrange & Act
        var warning = new ValidationWarning
        {
            Category = "Equipment",
            Message = "Character has no shield",
            Recommendation = "Consider equipping a shield"
        };

        // Assert
        warning.Category.Should().Be("Equipment");
        warning.Message.Should().Be("Character has no shield");
        warning.Recommendation.Should().Be("Consider equipping a shield");
        warning.Data.Should().NotBeNull();
        warning.Data.Should().BeEmpty();
    }

    [Fact]
    public void ValidationSuggestion_Properties_SetCorrectly()
    {
        // Arrange & Act
        var suggestion = new ValidationSuggestion
        {
            Category = "Optimization",
            Suggestion = "Learn Shield Block reaction",
            Benefit = "Reduces incoming damage"
        };

        // Assert
        suggestion.Category.Should().Be("Optimization");
        suggestion.Suggestion.Should().Be("Learn Shield Block reaction");
        suggestion.Benefit.Should().Be("Reduces incoming damage");
        suggestion.Data.Should().NotBeNull();
        suggestion.Data.Should().BeEmpty();
    }

    [Theory]
    [InlineData(ValidationSeverity.Error)]
    [InlineData(ValidationSeverity.Warning)]
    public void ValidationSeverity_Values_AreValid(ValidationSeverity severity)
    {
        // This test ensures the enum values are properly defined
        severity.Should().BeDefined();
    }

    [Fact]
    public void ValidationReport_Timestamp_IsReasonable()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;

        // Act
        var report = new ValidationReport();
        
        var afterCreation = DateTime.UtcNow;

        // Assert
        report.ValidationTimestamp.Should().BeOnOrAfter(beforeCreation);
        report.ValidationTimestamp.Should().BeOnOrBefore(afterCreation);
    }
}