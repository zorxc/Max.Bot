using FluentAssertions;
using Max.Bot.Configuration;
using Xunit;

namespace Max.Bot.Tests.Unit.Configuration;

public class MaxBotOptionsTests
{
    [Fact]
    public void MaxBotOptions_ShouldHaveDefaultValues()
    {
        // Act
        var options = new MaxBotOptions();

        // Assert
        options.Token.Should().BeEmpty();
        options.BaseUrl.Should().Be("https://platform-api.max.ru");
    }

    [Fact]
    public void MaxBotOptions_ShouldValidate_WithValidValues()
    {
        // Arrange
        var options = new MaxBotOptions
        {
            Token = "test-token-123",
            BaseUrl = "https://api.max.ru/bot"
        };

        // Act
        var act = () => options.Validate();

        // Assert
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void MaxBotOptions_Validate_ShouldThrow_WhenTokenIsNullOrEmpty(string? token)
    {
        // Arrange
        var options = new MaxBotOptions
        {
            Token = token!,
            BaseUrl = "https://api.max.ru/bot"
        };

        // Act
        var act = () => options.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("Token")
            .WithMessage("*cannot be null or empty*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void MaxBotOptions_Validate_ShouldThrow_WhenBaseUrlIsNullOrEmpty(string? baseUrl)
    {
        // Arrange
        var options = new MaxBotOptions
        {
            Token = "test-token-123",
            BaseUrl = baseUrl!
        };

        // Act
        var act = () => options.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("BaseUrl")
            .WithMessage("*cannot be null or empty*");
    }

    [Theory]
    [InlineData("not-a-uri")]
    [InlineData("relative/path")]
    public void MaxBotOptions_Validate_ShouldThrow_WhenBaseUrlIsInvalid(string baseUrl)
    {
        // Arrange
        var options = new MaxBotOptions
        {
            Token = "test-token-123",
            BaseUrl = baseUrl
        };

        // Act
        var act = () => options.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("BaseUrl");
    }

    [Theory]
    [InlineData("ftp://invalid.com")]
    [InlineData("file:///path/to/file")]
    public void MaxBotOptions_Validate_ShouldThrow_WhenBaseUrlUsesInvalidScheme(string baseUrl)
    {
        // Arrange
        var options = new MaxBotOptions
        {
            Token = "test-token-123",
            BaseUrl = baseUrl
        };

        // Act
        var act = () => options.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("BaseUrl")
            .WithMessage("*must use HTTP or HTTPS scheme*");
    }

    [Fact]
    public void MaxBotOptions_Validate_ShouldNotThrow_WithValidHttpUrl()
    {
        // Arrange
        var options = new MaxBotOptions
        {
            Token = "test-token-123",
            BaseUrl = "http://localhost:8080/bot"
        };

        // Act
        var act = () => options.Validate();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void MaxBotOptions_Validate_ShouldNotThrow_WithValidHttpsUrl()
    {
        // Arrange
        var options = new MaxBotOptions
        {
            Token = "test-token-123",
            BaseUrl = "https://api.max.ru/bot"
        };

        // Act
        var act = () => options.Validate();

        // Assert
        act.Should().NotThrow();
    }
}

