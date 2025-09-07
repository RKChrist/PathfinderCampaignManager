using PathfinderCampaignManager.Domain.Entities;
using PathfinderCampaignManager.Domain.Enums;
using PathfinderCampaignManager.Domain.Services;
using Xunit;

namespace PathfinderCampaignManager.Domain.Tests.Services;

public class ModifierEngineSimpleTests
{
    private readonly ModifierEngine _modifierEngine;

    public ModifierEngineSimpleTests()
    {
        _modifierEngine = new ModifierEngine();
    }

    [Fact]
    public void CalculateCharacterStats_WithNoModifiers_ReturnsBaseStats()
    {
        var character = Character.Create(Guid.NewGuid(), "Test Character", "fighter", "human", "acolyte");
        var modifiers = new List<CustomDefinitionModifier>();

        var result = _modifierEngine.CalculateCharacterStats(character, modifiers);

        Assert.NotNull(result);
        Assert.Equal(character.Id, result.CharacterId);
        Assert.True(result.BaseStats.Count > 0);
        Assert.Equal(10, result.BaseStats[ModifierTarget.Strength]);
        Assert.Equal(10, result.FinalStats[ModifierTarget.Strength]);
    }

    [Fact]
    public void CalculateCharacterStats_WithUntypedModifiers_StacksAll()
    {
        var character = Character.Create(Guid.NewGuid(), "Test Character", "fighter", "human", "acolyte");
        var modifiers = new List<CustomDefinitionModifier>
        {
            CreateModifier(ModifierTarget.Strength, 2, ModifierType.Untyped),
            CreateModifier(ModifierTarget.Strength, 3, ModifierType.Untyped),
            CreateModifier(ModifierTarget.Strength, 1, ModifierType.Untyped)
        };

        var result = _modifierEngine.CalculateCharacterStats(character, modifiers);

        Assert.Equal(10, result.BaseStats[ModifierTarget.Strength]);
        Assert.Equal(16, result.FinalStats[ModifierTarget.Strength]); // 10 + 2 + 3 + 1
        Assert.Equal(6, result.GetModifier(ModifierTarget.Strength)); // 2 + 3 + 1
    }

    [Fact]
    public void CalculateCharacterStats_WithTypedBonuses_OnlyHighestApplies()
    {
        var character = Character.Create(Guid.NewGuid(), "Test Character", "fighter", "human", "acolyte");
        var modifiers = new List<CustomDefinitionModifier>
        {
            CreateModifier(ModifierTarget.Strength, 2, ModifierType.Enhancement),
            CreateModifier(ModifierTarget.Strength, 4, ModifierType.Enhancement),
            CreateModifier(ModifierTarget.Strength, 3, ModifierType.Enhancement)
        };

        var result = _modifierEngine.CalculateCharacterStats(character, modifiers);

        Assert.Equal(10, result.BaseStats[ModifierTarget.Strength]);
        Assert.Equal(14, result.FinalStats[ModifierTarget.Strength]); // 10 + 4 (highest)
        Assert.Equal(4, result.GetModifier(ModifierTarget.Strength)); // Only highest applies
        Assert.True(result.Modifiers[ModifierTarget.Strength].StackingWarnings.Any());
    }

    [Fact]
    public void CalculateCharacterStats_WithDifferentTypes_AllStack()
    {
        var character = Character.Create(Guid.NewGuid(), "Test Character", "fighter", "human", "acolyte");
        var modifiers = new List<CustomDefinitionModifier>
        {
            CreateModifier(ModifierTarget.Strength, 2, ModifierType.Enhancement),
            CreateModifier(ModifierTarget.Strength, 1, ModifierType.Morale),
            CreateModifier(ModifierTarget.Strength, 3, ModifierType.Status),
            CreateModifier(ModifierTarget.Strength, 1, ModifierType.Untyped)
        };

        var result = _modifierEngine.CalculateCharacterStats(character, modifiers);

        Assert.Equal(10, result.BaseStats[ModifierTarget.Strength]);
        Assert.Equal(17, result.FinalStats[ModifierTarget.Strength]); // 10 + 2 + 1 + 3 + 1
        Assert.Equal(7, result.GetModifier(ModifierTarget.Strength)); // 2 + 1 + 3 + 1
    }

    private static CustomDefinitionModifier CreateModifier(ModifierTarget target, int value, ModifierType type)
    {
        return new CustomDefinitionModifier
        {
            CustomDefinitionId = Guid.NewGuid(),
            Target = target,
            Value = value,
            ModifierType = type,
            IsActive = true,
            Priority = 0
        };
    }
}