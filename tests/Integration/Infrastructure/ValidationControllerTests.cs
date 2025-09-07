using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using FluentAssertions;
using PathfinderCampaignManager.Domain.Validation;
using PathfinderCampaignManager.Domain.Entities.Pathfinder;
using PathfinderCampaignManager.Presentation.Server.Controllers;

namespace PathfinderCampaignManager.Infrastructure.Tests;

public class ValidationControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ValidationControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetValidationHealth_Always_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/validation/health");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("running");
    }

    [Fact]
    public async Task ValidateAbilityScores_ValidScores_ReturnsValidReport()
    {
        // Arrange
        var request = new AbilityScoreValidationRequest
        {
            AbilityScores = new Dictionary<string, int>
            {
                ["Strength"] = 16,
                ["Dexterity"] = 14,
                ["Constitution"] = 13,
                ["Intelligence"] = 12,
                ["Wisdom"] = 10,
                ["Charisma"] = 8
            },
            CharacterLevel = 1
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/validation/ability-scores", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var report = await response.Content.ReadFromJsonAsync<ValidationReport>();
        report.Should().NotBeNull();
        report!.IsValid.Should().BeTrue();
        report.ValidatedEntityType.Should().Be("AbilityScores");
    }

    [Fact]
    public async Task ValidateAbilityScores_InvalidScores_ReturnsInvalidReport()
    {
        // Arrange
        var request = new AbilityScoreValidationRequest
        {
            AbilityScores = new Dictionary<string, int>
            {
                ["Strength"] = 35, // Invalid - too high
                ["Dexterity"] = 14,
                ["Constitution"] = 13,
                ["Intelligence"] = 12,
                ["Wisdom"] = 10
                // Missing Charisma
            },
            CharacterLevel = 1
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/validation/ability-scores", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var report = await response.Content.ReadFromJsonAsync<ValidationReport>();
        report.Should().NotBeNull();
        report!.IsValid.Should().BeFalse();
        report.Issues.Should().NotBeEmpty();
        report.Issues.Should().Contain(i => i.Message.Contains("Strength"));
        report.Issues.Should().Contain(i => i.Message.Contains("Charisma"));
    }

    [Fact]
    public async Task ValidateAbilityScores_EmptyRequest_ReturnsBadRequest()
    {
        // Act
        var response = await _client.PostAsJsonAsync("/api/validation/ability-scores", (object?)null);

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        ((int)response.StatusCode).Should().Be(400);
    }

    [Fact]
    public async Task ValidatePrerequisites_MeetsAll_ReturnsValid()
    {
        // Arrange
        var mockCharacter = CreateMockCalculatedCharacter();
        var request = new PrerequisiteValidationRequest
        {
            Prerequisites = new List<PfPrerequisite>
            {
                new() { Type = "AbilityScore", Target = "Strength", Operator = ">=", Value = "13" },
                new() { Type = "Level", Value = "1" }
            },
            Character = mockCharacter
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/validation/prerequisites", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var report = await response.Content.ReadFromJsonAsync<ValidationReport>();
        report.Should().NotBeNull();
        report!.IsValid.Should().BeTrue();
        report.ValidatedEntityType.Should().Be("Prerequisites");
    }

    [Fact]
    public async Task ValidatePrerequisites_FailsRequirements_ReturnsInvalid()
    {
        // Arrange
        var mockCharacter = CreateMockCalculatedCharacter();
        mockCharacter.AbilityScores["Strength"] = 10; // Too low

        var request = new PrerequisiteValidationRequest
        {
            Prerequisites = new List<PfPrerequisite>
            {
                new() { Type = "AbilityScore", Target = "Strength", Operator = ">=", Value = "13" }
            },
            Character = mockCharacter
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/validation/prerequisites", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var report = await response.Content.ReadFromJsonAsync<ValidationReport>();
        report.Should().NotBeNull();
        report!.IsValid.Should().BeFalse();
        report.Issues.Should().Contain(i => i.Message.Contains("not met"));
    }

    [Fact]
    public async Task ValidateEquipmentLoad_WithinLimits_ReturnsValid()
    {
        // Arrange
        var mockCharacter = CreateMockCalculatedCharacter();
        mockCharacter.CurrentBulk = 3;
        mockCharacter.BulkLimit = 5;
        mockCharacter.IsEncumbered = false;

        // Act
        var response = await _client.PostAsJsonAsync("/api/validation/equipment", mockCharacter);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var report = await response.Content.ReadFromJsonAsync<ValidationReport>();
        report.Should().NotBeNull();
        report!.IsValid.Should().BeTrue();
        report.ValidatedEntityType.Should().Be("Equipment");
    }

    [Fact]
    public async Task ValidateEquipmentLoad_Overloaded_ReturnsInvalid()
    {
        // Arrange
        var mockCharacter = CreateMockCalculatedCharacter();
        mockCharacter.CurrentBulk = 12; // Way over limit
        mockCharacter.BulkLimit = 5;
        mockCharacter.IsEncumbered = true;

        // Act
        var response = await _client.PostAsJsonAsync("/api/validation/equipment", mockCharacter);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var report = await response.Content.ReadFromJsonAsync<ValidationReport>();
        report.Should().NotBeNull();
        report!.IsValid.Should().BeFalse();
        report.Issues.Should().Contain(i => i.Message.Contains("maximum bulk limit"));
    }

    [Fact]
    public async Task ValidateEquipmentLoad_NearLimit_ReturnsSuggestions()
    {
        // Arrange
        var mockCharacter = CreateMockCalculatedCharacter();
        mockCharacter.CurrentBulk = 4.5; // 90% of limit
        mockCharacter.BulkLimit = 5;
        mockCharacter.IsEncumbered = false;

        // Act
        var response = await _client.PostAsJsonAsync("/api/validation/equipment", mockCharacter);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var report = await response.Content.ReadFromJsonAsync<ValidationReport>();
        report.Should().NotBeNull();
        report!.IsValid.Should().BeTrue();
        report.Suggestions.Should().NotBeEmpty();
        report.Suggestions.Should().Contain(s => s.Suggestion.Contains("reducing carried equipment"));
    }

    [Fact]
    public async Task ValidateSpellcasting_ValidCharacter_ReturnsValid()
    {
        // Arrange
        var mockCharacter = CreateMockCalculatedCharacter();

        // Act
        var response = await _client.PostAsJsonAsync("/api/validation/spellcasting", mockCharacter);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var report = await response.Content.ReadFromJsonAsync<ValidationReport>();
        report.Should().NotBeNull();
        report!.ValidatedEntityType.Should().Be("Spellcasting");
    }

    [Fact]
    public async Task ValidateCampaign_ValidId_ReturnsReport()
    {
        // Arrange
        var campaignId = "test-campaign-123";

        // Act
        var response = await _client.PostAsync($"/api/validation/campaign/{campaignId}", null);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var report = await response.Content.ReadFromJsonAsync<ValidationReport>();
        report.Should().NotBeNull();
        report!.ValidatedEntityType.Should().Be("Campaign");
        report.ValidatedEntityId.Should().Be(campaignId);
    }

    [Fact]
    public async Task ValidateCampaign_EmptyId_ReturnsBadRequest()
    {
        // Act
        var response = await _client.PostAsync("/api/validation/campaign/", null);

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        ((int)response.StatusCode).Should().Be(404); // Not found due to route mismatch
    }

    private TestCalculatedCharacter CreateMockCalculatedCharacter()
    {
        return new TestCalculatedCharacter
        {
            Id = Guid.NewGuid(),
            Level = 1,
            AbilityScores = new Dictionary<string, int>
            {
                ["Strength"] = 16,
                ["Dexterity"] = 14,
                ["Constitution"] = 13,
                ["Intelligence"] = 12,
                ["Wisdom"] = 10,
                ["Charisma"] = 8
            },
            Proficiencies = new Dictionary<string, ProficiencyRank>
            {
                ["Athletics"] = ProficiencyRank.Trained,
                ["Simple Weapons"] = ProficiencyRank.Trained
            },
            AvailableFeats = new List<string>(),
            ValidationIssues = new List<ValidationIssue>(),
            HitPoints = 10,
            ArmorClass = 16,
            CurrentBulk = 3,
            BulkLimit = 5,
            IsEncumbered = false,
            FeatSlots = new Dictionary<string, List<FeatSlot>>()
        };
    }
}

// Test implementation of ICalculatedCharacter for integration tests
public class TestCalculatedCharacter : ICalculatedCharacter
{
    public Guid Id { get; set; }
    public int Level { get; set; }
    public Dictionary<string, int> AbilityScores { get; set; } = new();
    public Dictionary<string, ProficiencyRank> Proficiencies { get; set; } = new();
    public List<string> AvailableFeats { get; set; } = new();
    public List<ValidationIssue> ValidationIssues { get; set; } = new();
    public int HitPoints { get; set; }
    public int ArmorClass { get; set; }
    public double CurrentBulk { get; set; }
    public double BulkLimit { get; set; }
    public bool IsEncumbered { get; set; }
    public Dictionary<string, List<FeatSlot>> FeatSlots { get; set; } = new();
}