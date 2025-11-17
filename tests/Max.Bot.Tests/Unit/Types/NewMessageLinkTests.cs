using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using FluentAssertions;
using Max.Bot.Types.Requests;
using Xunit;

namespace Max.Bot.Tests.Unit.Types;

/// <summary>
/// Unit tests for <see cref="NewMessageLink"/>.
/// </summary>
public class NewMessageLinkTests
{
    [Fact]
    public void Id_ShouldBeGreaterThanZero()
    {
        // Arrange
        var link = new NewMessageLink
        {
            Id = 0,
            ChatId = null
        };

        // Act
        var validationResults = ValidateModel(link);

        // Assert
        validationResults.Should().Contain(v => v.MemberNames.Contains("Id") && v.ErrorMessage != null && v.ErrorMessage.Contains("greater than zero", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ChatId_ShouldBeGreaterThanZero_WhenProvided()
    {
        // Arrange
        var link = new NewMessageLink
        {
            Id = 1,
            ChatId = 0
        };

        // Act
        var validationResults = ValidateModel(link);

        // Assert
        validationResults.Should().Contain(v => v.MemberNames.Contains("ChatId") && v.ErrorMessage != null && v.ErrorMessage.Contains("greater than zero", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ChatId_CanBeNull()
    {
        // Arrange
        var link = new NewMessageLink
        {
            Id = 1,
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
            Id = 12345,
            ChatId = 67890
        };

        // Act
        var json = JsonSerializer.Serialize(link);

        // Assert
        json.Should().Contain("\"id\":12345");
        json.Should().Contain("\"chat_id\":67890");
    }

    [Fact]
    public void ShouldSerializeCorrectly_WithoutChatId()
    {
        // Arrange
        var link = new NewMessageLink
        {
            Id = 12345,
            ChatId = null
        };

        // Act
        var json = JsonSerializer.Serialize(link);

        // Assert
        json.Should().Contain("\"id\":12345");
        json.Should().Contain("\"chat_id\":null");
    }

    private static List<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(model);
        Validator.TryValidateObject(model, validationContext, validationResults, true);
        return validationResults;
    }
}

