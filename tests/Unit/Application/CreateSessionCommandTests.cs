using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using PathfinderCampaignManager.Application.Abstractions;
using PathfinderCampaignManager.Application.Sessions.Commands;
using PathfinderCampaignManager.Domain.Enums;
using PathfinderCampaignManager.Domain.Interfaces;
using NSubstitute;
using MediatR;
using PathfinderCampaignManager.Domain.Entities;

namespace PathfinderCampaignManager.Application.Tests;

public class CreateSessionCommandTests
{
    [Fact]
    public async Task Handle_ShouldCreateSession_WhenValidCommand()
    {
        // Arrange
        var mockCurrentUser = Substitute.For<ICurrentUserService>();
        var mockUnitOfWork = Substitute.For<IUnitOfWork>();
        var mockSessionRepo = Substitute.For<ISessionRepository>();

        var userId = Guid.NewGuid();
        mockCurrentUser.UserId.Returns(userId);
        mockUnitOfWork.Sessions.Returns(mockSessionRepo);

        var handler = new CreateSessionCommandHandler(mockUnitOfWork, mockCurrentUser);
        var command = new CreateSessionCommand("Test Session", "A test session");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);
        
        await mockSessionRepo.Received(1).AddAsync(Arg.Any<Session>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Validate_ShouldFailValidation_WhenNameIsEmpty()
    {
        // Arrange
        var validator = new CreateSessionCommandValidator();
        var command = new CreateSessionCommand("", "Description");

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Name));
    }

    [Fact]
    public void Validate_ShouldFailValidation_WhenNameIsTooLong()
    {
        // Arrange
        var validator = new CreateSessionCommandValidator();
        var longName = new string('x', 101); // 101 characters
        var command = new CreateSessionCommand(longName, "Description");

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Name));
    }

    [Fact]
    public async Task Authorize_ShouldSucceed_WhenUserIsAuthenticated()
    {
        // Arrange
        var mockCurrentUser = Substitute.For<ICurrentUserService>();
        var mockAuthService = Substitute.For<IAuthorizationService>();
        
        mockCurrentUser.IsAuthenticated.Returns(true);
        
        var command = new CreateSessionCommand("Test Session");

        // Act
        var result = await command.AuthorizeAsync(mockCurrentUser, mockAuthService, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}