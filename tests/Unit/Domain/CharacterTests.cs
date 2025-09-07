using FluentAssertions;
using PathfinderCampaignManager.Domain.Entities;
using PathfinderCampaignManager.Domain.Enums;

namespace PathfinderCampaignManager.Domain.Tests;

public class CharacterTests
{
    [Fact]
    public void Create_ShouldCreateCharacterWithValidProperties()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var name = "Aragorn";
        var classRef = "fighter";
        var ancestryRef = "human";
        var backgroundRef = "soldier";

        // Act
        var character = Character.Create(ownerId, name, classRef, ancestryRef, backgroundRef);

        // Assert
        character.Should().NotBeNull();
        character.Id.Should().NotBe(Guid.Empty);
        character.OwnerUserId.Should().Be(ownerId);
        character.Name.Should().Be(name);
        character.ClassRef.Should().Be(classRef);
        character.AncestryRef.Should().Be(ancestryRef);
        character.BackgroundRef.Should().Be(backgroundRef);
        character.Level.Should().Be(1);
        character.Visibility.Should().Be(CharacterVisibility.Private);
        character.SessionId.Should().BeNull();
        character.AuditLogs.Should().HaveCount(1);
        character.DomainEvents.Should().HaveCount(1);
        character.DomainEvents.First().Should().BeOfType<CharacterCreatedEvent>();
    }

    [Fact]
    public void UpdateLevel_ShouldUpdateLevelAndCreateAuditLog()
    {
        // Arrange
        var character = Character.Create(Guid.NewGuid(), "Test Character", "fighter", "human", "soldier");
        var updatedBy = Guid.NewGuid();
        var newLevel = 5;

        // Act
        character.UpdateLevel(newLevel, updatedBy);

        // Assert
        character.Level.Should().Be(newLevel);
        character.AuditLogs.Should().HaveCount(2); // Creation + Update
        var levelChangeLog = character.AuditLogs.Last();
        levelChangeLog.Description.Should().Contain("Level changed from 1 to 5");
        levelChangeLog.UserId.Should().Be(updatedBy);
        character.DomainEvents.Should().HaveCount(2);
        character.DomainEvents.Last().Should().BeOfType<CharacterLevelChangedEvent>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(21)]
    [InlineData(-1)]
    public void UpdateLevel_ShouldThrowException_WhenLevelIsInvalid(int invalidLevel)
    {
        // Arrange
        var character = Character.Create(Guid.NewGuid(), "Test Character", "fighter", "human", "soldier");

        // Act & Assert
        var action = () => character.UpdateLevel(invalidLevel, Guid.NewGuid());
        action.Should().Throw<ArgumentException>()
            .WithMessage("Level must be between 1 and 20");
    }

    [Fact]
    public void AssignToSession_ShouldAssignCharacterToSession()
    {
        // Arrange
        var character = Character.Create(Guid.NewGuid(), "Test Character", "fighter", "human", "soldier");
        var sessionId = Guid.NewGuid();

        // Act
        character.AssignToSession(sessionId);

        // Assert
        character.SessionId.Should().Be(sessionId);
        character.AuditLogs.Should().HaveCount(2); // Creation + Assignment
        var assignmentLog = character.AuditLogs.Last();
        assignmentLog.Description.Should().Contain($"Assigned to session {sessionId}");
        character.DomainEvents.Should().HaveCount(2);
        character.DomainEvents.Last().Should().BeOfType<CharacterAssignedToSessionEvent>();
    }

    [Fact]
    public void AssignToSession_ShouldThrowException_WhenAlreadyAssigned()
    {
        // Arrange
        var character = Character.Create(Guid.NewGuid(), "Test Character", "fighter", "human", "soldier");
        var sessionId = Guid.NewGuid();
        character.AssignToSession(sessionId);

        // Act & Assert
        var action = () => character.AssignToSession(Guid.NewGuid());
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Character is already assigned to a session");
    }

    [Fact]
    public void RemoveFromSession_ShouldRemoveSessionAssignment()
    {
        // Arrange
        var character = Character.Create(Guid.NewGuid(), "Test Character", "fighter", "human", "soldier");
        var sessionId = Guid.NewGuid();
        var removedBy = Guid.NewGuid();
        character.AssignToSession(sessionId);

        // Act
        character.RemoveFromSession(removedBy);

        // Assert
        character.SessionId.Should().BeNull();
        character.AuditLogs.Should().HaveCount(3); // Creation + Assignment + Removal
        var removalLog = character.AuditLogs.Last();
        removalLog.Description.Should().Contain($"Removed from session {sessionId}");
        removalLog.UserId.Should().Be(removedBy);
        character.DomainEvents.Should().HaveCount(3);
        character.DomainEvents.Last().Should().BeOfType<CharacterRemovedFromSessionEvent>();
    }
}