using FluentAssertions;
using PathfinderCampaignManager.Domain.Entities;
using PathfinderCampaignManager.Domain.Enums;

namespace PathfinderCampaignManager.Domain.Tests;

public class SessionTests
{
    [Fact]
    public void Create_ShouldCreateSessionWithValidProperties()
    {
        // Arrange
        var dmUserId = Guid.NewGuid();
        var name = "Test Campaign";
        var description = "A test campaign";

        // Act
        var session = Session.Create(dmUserId, name, description);

        // Assert
        session.Should().NotBeNull();
        session.Id.Should().NotBe(Guid.Empty);
        session.DMUserId.Should().Be(dmUserId);
        session.Name.Should().Be(name);
        session.Description.Should().Be(description);
        session.IsActive.Should().BeTrue();
        session.Code.Should().NotBeNull();
        session.Code.Value.Should().HaveLength(6);
        session.Members.Should().HaveCount(1);
        session.Members.First().Role.Should().Be(SessionMemberRole.DM);
        session.DomainEvents.Should().HaveCount(2);
        session.DomainEvents.Should().Contain(e => e is SessionCreatedEvent);
        session.DomainEvents.Should().Contain(e => e is SessionMemberJoinedEvent);
    }

    [Fact]
    public void AddMember_ShouldAddPlayerMember()
    {
        // Arrange
        var session = Session.Create(Guid.NewGuid(), "Test Campaign");
        var playerId = Guid.NewGuid();
        var alias = "TestPlayer";

        // Act
        session.AddMember(playerId, SessionMemberRole.Player, alias);

        // Assert
        session.Members.Should().HaveCount(2);
        var playerMember = session.Members.FirstOrDefault(m => m.UserId == playerId);
        playerMember.Should().NotBeNull();
        playerMember!.Role.Should().Be(SessionMemberRole.Player);
        playerMember.Alias.Should().Be(alias);
    }

    [Fact]
    public void AddMember_ShouldThrowException_WhenUserAlreadyExists()
    {
        // Arrange
        var dmUserId = Guid.NewGuid();
        var session = Session.Create(dmUserId, "Test Campaign");

        // Act & Assert
        var action = () => session.AddMember(dmUserId, SessionMemberRole.Player, "DuplicateUser");
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("User is already a member of this session");
    }

    [Fact]
    public void AddMember_ShouldThrowException_WhenTryingToAddSecondDM()
    {
        // Arrange
        var session = Session.Create(Guid.NewGuid(), "Test Campaign");
        var secondDMId = Guid.NewGuid();

        // Act & Assert
        var action = () => session.AddMember(secondDMId, SessionMemberRole.DM, "SecondDM");
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Session already has a DM");
    }

    [Fact]
    public void RemoveMember_ShouldRemovePlayerMember()
    {
        // Arrange
        var session = Session.Create(Guid.NewGuid(), "Test Campaign");
        var playerId = Guid.NewGuid();
        session.AddMember(playerId, SessionMemberRole.Player, "TestPlayer");

        // Act
        session.RemoveMember(playerId);

        // Assert
        session.Members.Should().HaveCount(1); // Only DM remains
        session.Members.Should().NotContain(m => m.UserId == playerId);
    }

    [Fact]
    public void RemoveMember_ShouldThrowException_WhenTryingToRemoveDM()
    {
        // Arrange
        var dmUserId = Guid.NewGuid();
        var session = Session.Create(dmUserId, "Test Campaign");

        // Act & Assert
        var action = () => session.RemoveMember(dmUserId);
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot remove the DM from the session");
    }
}