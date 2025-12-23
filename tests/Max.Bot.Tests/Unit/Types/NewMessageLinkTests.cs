using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using FluentAssertions;
using Max.Bot.Types.Requests;

namespace Max.Bot.Tests.Unit.Types;

/// <summary>
/// Unit tests for <see cref="NewMessageLink"/>.
/// </summary>
public class NewMessageLinkTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Id_ShouldBeRequired(string? id)
    {
        // Arrange
        var link = new NewMessageLink
        {
            Id = id!,
            ChatId = null
        };

        // Act
        var validationResults = ValidateModel(link);

        // Assert
        validationResults.Should().Contain(v => v.MemberNames.Contains("Id") && v.ErrorMessage != null && v.ErrorMessage.Contains("required", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(123456)]
    public void ChatId_ShouldAcceptPositiveValues_ForPersonalChats(long chatId)
    {
        // Arrange
        var link = new NewMessageLink
        {
            Id = "1",
            ChatId = chatId
        };

        // Act
        var validationResults = ValidateModel(link);

        // Assert
        validationResults.Should().NotContain(v => v.MemberNames.Contains("ChatId"));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-123456)]
    public void ChatId_ShouldAcceptNegativeValues_ForGroupChats(long chatId)
    {
        // Arrange
        var link = new NewMessageLink
        {
            Id = "1",
            ChatId = chatId
        };

        // Act
        var validationResults = ValidateModel(link);

        // Assert
        validationResults.Should().NotContain(v => v.MemberNames.Contains("ChatId"));
    }

    [Fact]
    public void ChatId_CanBeNull()
    {
        // Arrange
        var link = new NewMessageLink
        {
            Id = "1",
            ChatId = null
        };

        // Act
        var validationResults = ValidateModel(link);

        // Assert
        validationResults.Should().NotContain(v => v.MemberNames.Contains("ChatId"));
    }

    [Fact]
    public void ShouldSerializeCorrectly()
    {
        // Arrange
        var link = new NewMessageLink
        {
            Id = "12345",
            ChatId = 67890
        };

        // Act
        var json = JsonSerializer.Serialize(link);

        // Assert
        json.Should().Contain("\"id\":\"12345\"");
        json.Should().Contain("\"chat_id\":67890");
    }

    [Fact]
    public void ShouldSerializeCorrectly_WithoutChatId()
    {
        // Arrange
        var link = new NewMessageLink
        {
            Id = "12345",
            ChatId = null
        };

        // Act
        var json = JsonSerializer.Serialize(link);

        // Assert
        json.Should().Contain("\"id\":\"12345\"");
        json.Should().Contain("\"chat_id\":null");
    }

    [Fact]
    public void ShouldSerializeCorrectly_WithNegativeChatId_ForGroupChats()
    {
        // Arrange
        var link = new NewMessageLink
        {
            Id = "12345",
            ChatId = -67890
        };

        // Act
        var json = JsonSerializer.Serialize(link);

        // Assert
        json.Should().Contain("\"id\":\"12345\"");
        json.Should().Contain("\"chat_id\":-67890");
    }

    private static List<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(model);
        Validator.TryValidateObject(model, validationContext, validationResults, true);
        return validationResults;
    }
}

