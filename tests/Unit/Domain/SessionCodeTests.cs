using FluentAssertions;
using PathfinderCampaignManager.Domain.ValueObjects;

namespace PathfinderCampaignManager.Domain.Tests;

public class SessionCodeTests
{
    [Fact]
    public void Generate_ShouldCreateValidSessionCode()
    {
        // Act
        var sessionCode = SessionCode.Generate();

        // Assert
        sessionCode.Should().NotBeNull();
        sessionCode.Value.Should().HaveLength(6);
        sessionCode.Value.Should().MatchRegex("^[A-Z0-9]{6}$");
    }

    [Fact]
    public void Generate_ShouldCreateUniqueCodes()
    {
        // Arrange
        var codes = new HashSet<string>();

        // Act
        for (int i = 0; i < 100; i++)
        {
            var code = SessionCode.Generate();
            codes.Add(code.Value);
        }

        // Assert
        codes.Should().HaveCount(100); // All codes should be unique
    }

    [Theory]
    [InlineData("ABC123")]
    [InlineData("XYZ789")]
    [InlineData("TEST01")]
    public void FromString_ShouldCreateSessionCodeFromValidString(string validCode)
    {
        // Act
        var sessionCode = SessionCode.FromString(validCode);

        // Assert
        sessionCode.Should().NotBeNull();
        sessionCode.Value.Should().Be(validCode.ToUpper());
    }

    [Theory]
    [InlineData("abc123")] // lowercase should be converted to uppercase
    public void FromString_ShouldConvertToUppercase(string lowerCaseCode)
    {
        // Act
        var sessionCode = SessionCode.FromString(lowerCaseCode);

        // Assert
        sessionCode.Value.Should().Be(lowerCaseCode.ToUpper());
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    [InlineData("ABC12")]  // too short
    [InlineData("ABC1234")] // too long
    public void FromString_ShouldThrowException_WhenCodeIsInvalid(string invalidCode)
    {
        // Act & Assert
        var action = () => SessionCode.FromString(invalidCode);
        action.Should().Throw<ArgumentException>()
            .WithMessage("Session code must be exactly 6 characters");
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var code = "ABC123";
        var sessionCode = SessionCode.FromString(code);

        // Act
        var result = sessionCode.ToString();

        // Assert
        result.Should().Be(code);
    }

    [Fact]
    public void ImplicitConversion_ShouldConvertToString()
    {
        // Arrange
        var code = "ABC123";
        var sessionCode = SessionCode.FromString(code);

        // Act
        string result = sessionCode;

        // Assert
        result.Should().Be(code);
    }
}