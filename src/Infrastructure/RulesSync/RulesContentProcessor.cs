using Microsoft.Extensions.Logging;
using PathfinderCampaignManager.Application.RulesSync.Models;
using PathfinderCampaignManager.Application.RulesSync.Services;
using PathfinderCampaignManager.Domain.Entities;
using PathfinderCampaignManager.Domain.Enums;
using PathfinderCampaignManager.Domain.Interfaces;
using System.Text.Json;

namespace PathfinderCampaignManager.Infrastructure.RulesSync;

public class RulesContentProcessor : IRulesContentProcessor
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RulesContentProcessor> _logger;

    public RulesContentProcessor(IUnitOfWork unitOfWork, ILogger<RulesContentProcessor> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ProcessingResult> ProcessClassesAsync(List<SrdContent> content, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing {Count} classes from SRD", content.Count);
        
        var result = new ProcessingResult { IsSuccess = true };
        var errors = new List<ProcessingError>();

        foreach (var srdContent in content)
        {
            if (cancellationToken.IsCancellationRequested) break;

            try
            {
                var existingClass = await FindExistingClass(srdContent.Name);
                
                if (existingClass != null)
                {
                    // Update existing class
                    await UpdateExistingClass(existingClass, srdContent);
                    result.ItemsUpdated++;
                }
                else
                {
                    // Create new class
                    await CreateNewClass(srdContent);
                    result.ItemsCreated++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process class: {Name}", srdContent.Name);
                result.ItemsFailed++;
                errors.Add(new ProcessingError
                {
                    ItemName = srdContent.Name,
                    ErrorMessage = ex.Message,
                    Type = ProcessingErrorType.DatabaseError
                });
            }
        }

        result.Errors = errors;
        result.IsSuccess = errors.Count == 0;

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Successfully processed classes. Created: {Created}, Updated: {Updated}, Failed: {Failed}", 
                result.ItemsCreated, result.ItemsUpdated, result.ItemsFailed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save class processing results");
            result.IsSuccess = false;
            result.Errors.Add(new ProcessingError
            {
                ItemName = "Database Save",
                ErrorMessage = ex.Message,
                Type = ProcessingErrorType.DatabaseError
            });
        }

        return result;
    }

    public async Task<ProcessingResult> ProcessSpellsAsync(List<SrdContent> content, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing {Count} spells from SRD", content.Count);
        
        var result = new ProcessingResult { IsSuccess = true };
        var errors = new List<ProcessingError>();

        foreach (var srdContent in content)
        {
            if (cancellationToken.IsCancellationRequested) break;

            try
            {
                var spell = await CreateSpellFromSrd(srdContent);
                if (spell != null)
                {
                    result.ItemsCreated++;
                }
                else
                {
                    result.ItemsSkipped++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process spell: {Name}", srdContent.Name);
                result.ItemsFailed++;
                errors.Add(new ProcessingError
                {
                    ItemName = srdContent.Name,
                    ErrorMessage = ex.Message,
                    Type = ProcessingErrorType.ParseError
                });
            }
        }

        result.Errors = errors;
        result.IsSuccess = errors.Count == 0;

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save spell processing results");
            result.IsSuccess = false;
        }

        return result;
    }

    public async Task<ProcessingResult> ProcessFeatsAsync(List<SrdContent> content, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing {Count} feats from SRD", content.Count);
        
        var result = new ProcessingResult { IsSuccess = true };
        var errors = new List<ProcessingError>();

        foreach (var srdContent in content)
        {
            if (cancellationToken.IsCancellationRequested) break;

            try
            {
                var feat = await CreateFeatFromSrd(srdContent);
                if (feat != null)
                {
                    result.ItemsCreated++;
                }
                else
                {
                    result.ItemsSkipped++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process feat: {Name}", srdContent.Name);
                result.ItemsFailed++;
                errors.Add(new ProcessingError
                {
                    ItemName = srdContent.Name,
                    ErrorMessage = ex.Message,
                    Type = ProcessingErrorType.ParseError
                });
            }
        }

        result.Errors = errors;
        result.IsSuccess = errors.Count == 0;

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save feat processing results");
            result.IsSuccess = false;
        }

        return result;
    }

    public async Task<ProcessingResult> ProcessEquipmentAsync(List<SrdContent> content, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing {Count} equipment items from SRD", content.Count);
        
        var result = new ProcessingResult { IsSuccess = true };
        var errors = new List<ProcessingError>();

        foreach (var srdContent in content)
        {
            if (cancellationToken.IsCancellationRequested) break;

            try
            {
                var equipment = await CreateEquipmentFromSrd(srdContent);
                if (equipment != null)
                {
                    result.ItemsCreated++;
                }
                else
                {
                    result.ItemsSkipped++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process equipment: {Name}", srdContent.Name);
                result.ItemsFailed++;
                errors.Add(new ProcessingError
                {
                    ItemName = srdContent.Name,
                    ErrorMessage = ex.Message,
                    Type = ProcessingErrorType.ParseError
                });
            }
        }

        result.Errors = errors;
        result.IsSuccess = errors.Count == 0;

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save equipment processing results");
            result.IsSuccess = false;
        }

        return result;
    }

    public async Task<ProcessingResult> ProcessAncestryAsync(List<SrdContent> content, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing {Count} ancestries from SRD", content.Count);
        
        var result = new ProcessingResult { IsSuccess = true };
        var errors = new List<ProcessingError>();

        foreach (var srdContent in content)
        {
            if (cancellationToken.IsCancellationRequested) break;

            try
            {
                var ancestry = await CreateAncestryFromSrd(srdContent);
                if (ancestry != null)
                {
                    result.ItemsCreated++;
                }
                else
                {
                    result.ItemsSkipped++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process ancestry: {Name}", srdContent.Name);
                result.ItemsFailed++;
                errors.Add(new ProcessingError
                {
                    ItemName = srdContent.Name,
                    ErrorMessage = ex.Message,
                    Type = ProcessingErrorType.ParseError
                });
            }
        }

        result.Errors = errors;
        result.IsSuccess = errors.Count == 0;

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save ancestry processing results");
            result.IsSuccess = false;
        }

        return result;
    }

    public async Task<ProcessingResult> ProcessBackgroundsAsync(List<SrdContent> content, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing {Count} backgrounds from SRD", content.Count);
        
        var result = new ProcessingResult { IsSuccess = true };
        var errors = new List<ProcessingError>();

        foreach (var srdContent in content)
        {
            if (cancellationToken.IsCancellationRequested) break;

            try
            {
                var background = await CreateBackgroundFromSrd(srdContent);
                if (background != null)
                {
                    result.ItemsCreated++;
                }
                else
                {
                    result.ItemsSkipped++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process background: {Name}", srdContent.Name);
                result.ItemsFailed++;
                errors.Add(new ProcessingError
                {
                    ItemName = srdContent.Name,
                    ErrorMessage = ex.Message,
                    Type = ProcessingErrorType.ParseError
                });
            }
        }

        result.Errors = errors;
        result.IsSuccess = errors.Count == 0;

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save background processing results");
            result.IsSuccess = false;
        }

        return result;
    }

    private async Task<CustomDefinition?> FindExistingClass(string name)
    {
        // This would use a proper repository to find existing classes
        // For now, return null to always create new
        await Task.CompletedTask;
        return null;
    }

    private async Task UpdateExistingClass(CustomDefinition existingClass, SrdContent srdContent)
    {
        // Update existing class with new data
        var jsonData = ConvertClassSrdToJson(srdContent);
        existingClass.UpdateContent(srdContent.Name, GenerateClassDescription(srdContent), jsonData, Guid.Empty);
        await Task.CompletedTask;
    }

    private async Task CreateNewClass(SrdContent srdContent)
    {
        var jsonData = ConvertClassSrdToJson(srdContent);
        var description = GenerateClassDescription(srdContent);

        var customDefinition = CustomDefinition.Create(
            Guid.Empty, // System user
            CustomDefinitionType.Class,
            srdContent.Name,
            description,
            jsonData,
            CustomCategories.CoreClass);

        customDefinition.UpdateMetadata(
            "Common",
            srdContent.Tags,
            1, // Classes are level 1
            CustomCategories.CoreClass,
            Guid.Empty);

        var repository = _unitOfWork.Repository<CustomDefinition>();
        await repository.AddAsync(customDefinition);
    }

    private async Task<CustomDefinition?> CreateSpellFromSrd(SrdContent srdContent)
    {
        try
        {
            var jsonData = ConvertSpellSrdToJson(srdContent);
            var description = GenerateSpellDescription(srdContent);
            var level = ExtractSpellLevel(srdContent);

            var customDefinition = CustomDefinition.Create(
                Guid.Empty,
                CustomDefinitionType.Spell,
                srdContent.Name,
                description,
                jsonData,
                CustomCategories.Spell);

            customDefinition.UpdateMetadata(
                "Common",
                srdContent.Tags,
                level,
                CustomCategories.Spell,
                Guid.Empty);

            var repository = _unitOfWork.Repository<CustomDefinition>();
            await repository.AddAsync(customDefinition);

            return customDefinition;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create spell from SRD: {Name}", srdContent.Name);
            return null;
        }
    }

    private async Task<CustomDefinition?> CreateFeatFromSrd(SrdContent srdContent)
    {
        try
        {
            var jsonData = ConvertFeatSrdToJson(srdContent);
            var description = GenerateFeatDescription(srdContent);
            var level = ExtractFeatLevel(srdContent);

            var customDefinition = CustomDefinition.Create(
                Guid.Empty,
                CustomDefinitionType.Feat,
                srdContent.Name,
                description,
                jsonData,
                CustomCategories.GeneralFeat);

            customDefinition.UpdateMetadata(
                "Common",
                srdContent.Tags,
                level,
                CustomCategories.GeneralFeat,
                Guid.Empty);

            var repository = _unitOfWork.Repository<CustomDefinition>();
            await repository.AddAsync(customDefinition);

            return customDefinition;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create feat from SRD: {Name}", srdContent.Name);
            return null;
        }
    }

    private async Task<CustomDefinition?> CreateEquipmentFromSrd(SrdContent srdContent)
    {
        try
        {
            var jsonData = ConvertEquipmentSrdToJson(srdContent);
            var description = GenerateEquipmentDescription(srdContent);
            var level = ExtractEquipmentLevel(srdContent);

            var customDefinition = CustomDefinition.Create(
                Guid.Empty,
                CustomDefinitionType.Item,
                srdContent.Name,
                description,
                jsonData,
                CustomCategories.Tool);

            customDefinition.UpdateMetadata(
                "Common",
                srdContent.Tags,
                level,
                CustomCategories.Tool,
                Guid.Empty);

            // Add modifiers for magic items
            await AddEquipmentModifiers(customDefinition, srdContent);

            var repository = _unitOfWork.Repository<CustomDefinition>();
            await repository.AddAsync(customDefinition);

            return customDefinition;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create equipment from SRD: {Name}", srdContent.Name);
            return null;
        }
    }

    private async Task<CustomDefinition?> CreateAncestryFromSrd(SrdContent srdContent)
    {
        try
        {
            var jsonData = ConvertAncestrySrdToJson(srdContent);
            var description = GenerateAncestryDescription(srdContent);

            var customDefinition = CustomDefinition.Create(
                Guid.Empty,
                CustomDefinitionType.Ancestry,
                srdContent.Name,
                description,
                jsonData,
                "Core Ancestry");

            customDefinition.UpdateMetadata(
                "Common",
                srdContent.Tags,
                0, // Ancestries don't have levels
                "Core Ancestry",
                Guid.Empty);

            var repository = _unitOfWork.Repository<CustomDefinition>();
            await repository.AddAsync(customDefinition);

            return customDefinition;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create ancestry from SRD: {Name}", srdContent.Name);
            return null;
        }
    }

    private async Task<CustomDefinition?> CreateBackgroundFromSrd(SrdContent srdContent)
    {
        try
        {
            var jsonData = ConvertBackgroundSrdToJson(srdContent);
            var description = GenerateBackgroundDescription(srdContent);

            var customDefinition = CustomDefinition.Create(
                Guid.Empty,
                CustomDefinitionType.Background,
                srdContent.Name,
                description,
                jsonData,
                "Core Background");

            customDefinition.UpdateMetadata(
                "Common",
                srdContent.Tags,
                0, // Backgrounds don't have levels
                "Core Background",
                Guid.Empty);

            var repository = _unitOfWork.Repository<CustomDefinition>();
            await repository.AddAsync(customDefinition);

            return customDefinition;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create background from SRD: {Name}", srdContent.Name);
            return null;
        }
    }

    private string ConvertClassSrdToJson(SrdContent srdContent)
    {
        var classData = new
        {
            type = "class",
            hitPoints = srdContent.ParsedData.GetValueOrDefault("hitPoints", "8"),
            keyAbility = srdContent.ParsedData.GetValueOrDefault("keyAbility", ""),
            proficiencies = srdContent.ParsedData.GetValueOrDefault("proficiencies", ""),
            classDC = srdContent.ParsedData.GetValueOrDefault("classDC", ""),
            skills = srdContent.ParsedData.GetValueOrDefault("skills", "2"),
            source = srdContent.Metadata.GetValueOrDefault("sourceBook", "Core Rulebook"),
            traits = srdContent.Tags
        };

        return JsonSerializer.Serialize(classData, new JsonSerializerOptions { WriteIndented = true });
    }

    private string ConvertSpellSrdToJson(SrdContent srdContent)
    {
        var spellData = new
        {
            type = "spell",
            traditions = srdContent.ParsedData.GetValueOrDefault("traditions", ""),
            cast = srdContent.ParsedData.GetValueOrDefault("cast", ""),
            range = srdContent.ParsedData.GetValueOrDefault("range", ""),
            area = srdContent.ParsedData.GetValueOrDefault("area", ""),
            duration = srdContent.ParsedData.GetValueOrDefault("duration", ""),
            level = int.Parse(srdContent.ParsedData.GetValueOrDefault("level", "1")?.ToString() ?? "1"),
            source = srdContent.Metadata.GetValueOrDefault("sourceBook", "Core Rulebook"),
            traits = srdContent.Tags
        };

        return JsonSerializer.Serialize(spellData, new JsonSerializerOptions { WriteIndented = true });
    }

    private string ConvertFeatSrdToJson(SrdContent srdContent)
    {
        var featData = new
        {
            type = "feat",
            prerequisites = srdContent.ParsedData.GetValueOrDefault("prerequisites", ""),
            frequency = srdContent.ParsedData.GetValueOrDefault("frequency", ""),
            trigger = srdContent.ParsedData.GetValueOrDefault("trigger", ""),
            requirements = srdContent.ParsedData.GetValueOrDefault("requirements", ""),
            level = int.Parse(srdContent.ParsedData.GetValueOrDefault("level", "1")?.ToString() ?? "1"),
            source = srdContent.Metadata.GetValueOrDefault("sourceBook", "Core Rulebook"),
            traits = srdContent.Tags
        };

        return JsonSerializer.Serialize(featData, new JsonSerializerOptions { WriteIndented = true });
    }

    private string ConvertEquipmentSrdToJson(SrdContent srdContent)
    {
        var equipmentData = new
        {
            type = "equipment",
            price = srdContent.ParsedData.GetValueOrDefault("price", ""),
            bulk = srdContent.ParsedData.GetValueOrDefault("bulk", ""),
            usage = srdContent.ParsedData.GetValueOrDefault("usage", ""),
            category = srdContent.ParsedData.GetValueOrDefault("category", ""),
            level = int.Parse(srdContent.ParsedData.GetValueOrDefault("level", "0")?.ToString() ?? "0"),
            source = srdContent.Metadata.GetValueOrDefault("sourceBook", "Core Rulebook"),
            traits = srdContent.Tags
        };

        return JsonSerializer.Serialize(equipmentData, new JsonSerializerOptions { WriteIndented = true });
    }

    private string ConvertAncestrySrdToJson(SrdContent srdContent)
    {
        var ancestryData = new
        {
            type = "ancestry",
            abilityBoosts = srdContent.ParsedData.GetValueOrDefault("abilityBoosts", ""),
            abilityFlaw = srdContent.ParsedData.GetValueOrDefault("abilityFlaw", ""),
            hitPoints = int.Parse(srdContent.ParsedData.GetValueOrDefault("hitPoints", "8")?.ToString() ?? "8"),
            size = srdContent.ParsedData.GetValueOrDefault("size", "Medium"),
            speed = srdContent.ParsedData.GetValueOrDefault("speed", "25 feet"),
            languages = srdContent.ParsedData.GetValueOrDefault("languages", ""),
            source = srdContent.Metadata.GetValueOrDefault("sourceBook", "Core Rulebook"),
            traits = srdContent.Tags
        };

        return JsonSerializer.Serialize(ancestryData, new JsonSerializerOptions { WriteIndented = true });
    }

    private string ConvertBackgroundSrdToJson(SrdContent srdContent)
    {
        var backgroundData = new
        {
            type = "background",
            abilityBoosts = srdContent.ParsedData.GetValueOrDefault("abilityBoosts", ""),
            skillFeats = srdContent.ParsedData.GetValueOrDefault("skillFeats", ""),
            lore = srdContent.ParsedData.GetValueOrDefault("lore", ""),
            feat = srdContent.ParsedData.GetValueOrDefault("feat", ""),
            source = srdContent.Metadata.GetValueOrDefault("sourceBook", "Core Rulebook"),
            traits = srdContent.Tags
        };

        return JsonSerializer.Serialize(backgroundData, new JsonSerializerOptions { WriteIndented = true });
    }

    private string GenerateClassDescription(SrdContent srdContent) =>
        $"Pathfinder 2e {srdContent.Name} class with {srdContent.ParsedData.GetValueOrDefault("hitPoints", "8")} Hit Points and {srdContent.ParsedData.GetValueOrDefault("keyAbility", "various")} as key ability.";

    private string GenerateSpellDescription(SrdContent srdContent) =>
        $"Level {srdContent.ParsedData.GetValueOrDefault("level", "1")} {srdContent.ParsedData.GetValueOrDefault("traditions", "arcane")} spell.";

    private string GenerateFeatDescription(SrdContent srdContent) =>
        $"Level {srdContent.ParsedData.GetValueOrDefault("level", "1")} feat from Pathfinder 2e.";

    private string GenerateEquipmentDescription(SrdContent srdContent) =>
        $"Equipment item with price {srdContent.ParsedData.GetValueOrDefault("price", "unknown")} and bulk {srdContent.ParsedData.GetValueOrDefault("bulk", "L")}.";

    private string GenerateAncestryDescription(SrdContent srdContent) =>
        $"Pathfinder 2e ancestry with {srdContent.ParsedData.GetValueOrDefault("hitPoints", "8")} Hit Points and {srdContent.ParsedData.GetValueOrDefault("speed", "25 feet")} Speed.";

    private string GenerateBackgroundDescription(SrdContent srdContent) =>
        $"Character background providing {srdContent.ParsedData.GetValueOrDefault("abilityBoosts", "ability boosts")} and {srdContent.ParsedData.GetValueOrDefault("lore", "Lore")} skill.";

    private int ExtractSpellLevel(SrdContent srdContent) =>
        int.TryParse(srdContent.ParsedData.GetValueOrDefault("level", "1")?.ToString(), out var level) ? level : 1;

    private int ExtractFeatLevel(SrdContent srdContent) =>
        int.TryParse(srdContent.ParsedData.GetValueOrDefault("level", "1")?.ToString(), out var level) ? level : 1;

    private int ExtractEquipmentLevel(SrdContent srdContent) =>
        int.TryParse(srdContent.ParsedData.GetValueOrDefault("level", "0")?.ToString(), out var level) ? level : 0;

    private async Task AddEquipmentModifiers(CustomDefinition customDefinition, SrdContent srdContent)
    {
        // Parse potential modifiers from equipment data
        // This is a simplified example - real implementation would need more sophisticated parsing
        
        var name = srdContent.Name.ToLowerInvariant();
        
        // Example: Belt of Giant Strength
        if (name.Contains("strength") && name.Contains("belt"))
        {
            customDefinition.AddModifier(ModifierTarget.Strength, 2, ModifierType.Item);
        }
        
        // Example: Ring of Protection
        if (name.Contains("protection") && name.Contains("ring"))
        {
            customDefinition.AddModifier(ModifierTarget.ArmorClass, 1, ModifierType.Deflection);
        }
        
        // Example: Cloak of Resistance
        if (name.Contains("resistance") && name.Contains("cloak"))
        {
            customDefinition.AddModifier(ModifierTarget.FortitudeSave, 1, ModifierType.Resistance);
            customDefinition.AddModifier(ModifierTarget.ReflexSave, 1, ModifierType.Resistance);
            customDefinition.AddModifier(ModifierTarget.WillSave, 1, ModifierType.Resistance);
        }
        
        await Task.CompletedTask;
    }
}