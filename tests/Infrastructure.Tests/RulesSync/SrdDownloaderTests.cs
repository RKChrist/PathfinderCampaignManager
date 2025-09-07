using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using PathfinderCampaignManager.Application.RulesSync.Models;
using PathfinderCampaignManager.Infrastructure.RulesSync;
using System.Net;
using Xunit;

namespace PathfinderCampaignManager.Infrastructure.Tests.RulesSync;

public class SrdDownloaderTests
{
    private readonly Mock<ILogger<SrdDownloader>> _mockLogger;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly SrdDownloader _srdDownloader;
    private readonly SrdConfiguration _config;

    public SrdDownloaderTests()
    {
        _mockLogger = new Mock<ILogger<SrdDownloader>>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        _config = new SrdConfiguration();
        
        _srdDownloader = new SrdDownloader(_httpClient, _mockLogger.Object, _config);
    }

    [Fact]
    public async Task DownloadContentAsync_WithValidUrl_ReturnsContent()
    {
        // Arrange
        var url = "https://2e.aonprd.com/Classes.aspx?ID=1";
        var htmlContent = @"
            <html>
                <head><title>Fighter - Archives of Nethys: Pathfinder 2nd Edition Database</title></head>
                <body>
                    <h1>Fighter</h1>
                    <p>Hit Points: 10</p>
                    <p>Key Ability: Strength or Dexterity</p>
                    <div class='traits'>
                        <span class='trait'>Martial</span>
                    </div>
                </body>
            </html>";

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(htmlContent)
            });

        // Act
        var result = await _srdDownloader.DownloadContentAsync(url);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Fighter", result.Name);
        Assert.Equal(ContentType.Classes, result.Type);
        Assert.Equal(url, result.SourceUrl);
        Assert.Contains("Hit Points", result.RawContent);
        Assert.Contains("10", result.ParsedData["hitPoints"]?.ToString());
        Assert.Contains("Strength or Dexterity", result.ParsedData["keyAbility"]?.ToString());
    }

    [Fact]
    public async Task DownloadContentAsync_WithSpellUrl_ParsesSpellData()
    {
        // Arrange
        var url = "https://2e.aonprd.com/Spells.aspx?ID=1";
        var htmlContent = @"
            <html>
                <head><title>Fireball</title></head>
                <body>
                    <h1>Fireball</h1>
                    <p>Traditions: Arcane, Primal</p>
                    <p>Cast: somatic, verbal</p>
                    <p>Range: 500 feet</p>
                    <p>Area: 20-foot burst</p>
                    <p>Duration: instantaneous</p>
                    <p>Spell 3</p>
                    <div class='traits'>
                        <span class='trait'>Evocation</span>
                        <span class='trait'>Fire</span>
                    </div>
                </body>
            </html>";

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(htmlContent)
            });

        // Act
        var result = await _srdDownloader.DownloadContentAsync(url);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Fireball", result.Name);
        Assert.Equal(ContentType.Spells, result.Type);
        Assert.Contains("Arcane, Primal", result.ParsedData["traditions"]?.ToString());
        Assert.Contains("somatic, verbal", result.ParsedData["cast"]?.ToString());
        Assert.Contains("3", result.ParsedData["level"]?.ToString());
        Assert.Contains("Evocation", result.Tags);
        Assert.Contains("Fire", result.Tags);
    }

    [Fact]
    public async Task DownloadContentAsync_WithEquipmentUrl_ParsesEquipmentData()
    {
        // Arrange
        var url = "https://2e.aonprd.com/Equipment.aspx?ID=1";
        var htmlContent = @"
            <html>
                <head><title>Longsword</title></head>
                <body>
                    <h1>Longsword</h1>
                    <p>Price: 1 gp</p>
                    <p>Bulk: 1</p>
                    <p>Usage: held in 1 hand</p>
                    <p>Category: Martial Melee Weapons</p>
                    <div class='traits'>
                        <span class='trait'>Versatile P</span>
                    </div>
                </body>
            </html>";

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(htmlContent)
            });

        // Act
        var result = await _srdDownloader.DownloadContentAsync(url);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Longsword", result.Name);
        Assert.Equal(ContentType.Equipment, result.Type);
        Assert.Contains("1 gp", result.ParsedData["price"]?.ToString());
        Assert.Contains("1", result.ParsedData["bulk"]?.ToString());
        Assert.Contains("held in 1 hand", result.ParsedData["usage"]?.ToString());
        Assert.Contains("Versatile P", result.Tags);
    }

    [Fact]
    public async Task DownloadContentAsync_WithNetworkError_ThrowsException()
    {
        // Arrange
        var url = "https://2e.aonprd.com/Classes.aspx?ID=1";

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => 
            _srdDownloader.DownloadContentAsync(url));
    }

    [Fact]
    public async Task ValidateContentAsync_WithValidClassContent_ReturnsValid()
    {
        // Arrange
        var content = new SrdContent
        {
            Id = "fighter",
            Name = "Fighter",
            Type = ContentType.Classes,
            RawContent = "<html><body>Fighter class content</body></html>",
            ParsedData = new Dictionary<string, object>
            {
                ["hitPoints"] = "10",
                ["keyAbility"] = "Strength"
            }
        };

        // Act
        var result = await _srdDownloader.ValidateContentAsync(content);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
        Assert.Equal(ContentType.Classes, result.DetectedType);
    }

    [Fact]
    public async Task ValidateContentAsync_WithEmptyName_ReturnsInvalid()
    {
        // Arrange
        var content = new SrdContent
        {
            Id = "test",
            Name = "",
            Type = ContentType.Classes,
            RawContent = "<html><body>Content</body></html>"
        };

        // Act
        var result = await _srdDownloader.ValidateContentAsync(content);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Field == "Name");
    }

    [Fact]
    public async Task ValidateContentAsync_WithEmptyContent_ReturnsInvalid()
    {
        // Arrange
        var content = new SrdContent
        {
            Id = "test",
            Name = "Test",
            Type = ContentType.Classes,
            RawContent = ""
        };

        // Act
        var result = await _srdDownloader.ValidateContentAsync(content);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Field == "RawContent");
    }

    [Fact]
    public async Task GetContentIndexAsync_ReturnsContentIndex()
    {
        // Arrange
        var classesIndexHtml = @"
            <html>
                <body>
                    <a href='Classes.aspx?ID=1'>Fighter</a>
                    <a href='Classes.aspx?ID=2'>Wizard</a>
                    <a href='Classes.aspx?ID=3'>Cleric</a>
                </body>
            </html>";

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains("Classes.aspx")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(classesIndexHtml)
            });

        // Act
        var result = await _srdDownloader.GetContentIndexAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains(result, item => item.Name == "Fighter" && item.Type == ContentType.Classes);
        Assert.Contains(result, item => item.Name == "Wizard" && item.Type == ContentType.Classes);
        Assert.Contains(result, item => item.Name == "Cleric" && item.Type == ContentType.Classes);
    }

    [Theory]
    [InlineData("https://2e.aonprd.com/Classes.aspx?ID=1", ContentType.Classes)]
    [InlineData("https://2e.aonprd.com/Spells.aspx?ID=1", ContentType.Spells)]
    [InlineData("https://2e.aonprd.com/Feats.aspx?ID=1", ContentType.Feats)]
    [InlineData("https://2e.aonprd.com/Equipment.aspx?ID=1", ContentType.Equipment)]
    [InlineData("https://2e.aonprd.com/Ancestries.aspx?ID=1", ContentType.Ancestry)]
    [InlineData("https://2e.aonprd.com/Backgrounds.aspx?ID=1", ContentType.Backgrounds)]
    public async Task DownloadContentAsync_DetectsCorrectContentType(string url, ContentType expectedType)
    {
        // Arrange
        var htmlContent = @"
            <html>
                <head><title>Test Content</title></head>
                <body><h1>Test Content</h1></body>
            </html>";

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(htmlContent)
            });

        // Act
        var result = await _srdDownloader.DownloadContentAsync(url);

        // Assert
        Assert.Equal(expectedType, result.Type);
    }

    [Fact]
    public async Task DownloadContentAsync_ExtractsTraitsCorrectly()
    {
        // Arrange
        var url = "https://2e.aonprd.com/Spells.aspx?ID=1";
        var htmlContent = @"
            <html>
                <body>
                    <h1>Test Spell</h1>
                    <span class='trait'>Evocation</span>
                    <span class='trait'>Fire</span>
                    <span class='trait'>Attack</span>
                </body>
            </html>";

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(htmlContent)
            });

        // Act
        var result = await _srdDownloader.DownloadContentAsync(url);

        // Assert
        Assert.Contains("Evocation", result.Tags);
        Assert.Contains("Fire", result.Tags);
        Assert.Contains("Attack", result.Tags);
        Assert.Equal(3, result.Tags.Count);
    }
}