using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace PathfinderCampaignManager.Presentation.Tests.Components;

public class SimpleComponentTests : TestContext
{
    public SimpleComponentTests()
    {
        Services.AddLogging();
    }

    [Fact]
    public void SimpleComponent_CanBeCreated()
    {
        // This is a basic test to verify our test setup works
        // Act & Assert
        Assert.True(true);
    }

    [Fact] 
    public void TestContext_CanRenderBasicHtml()
    {
        // Arrange & Act
        var component = Render(@"<div class=""test-div"">Hello World</div>");

        // Assert
        Assert.Contains("Hello World", component.Markup);
        Assert.Contains("test-div", component.Markup);
    }
}