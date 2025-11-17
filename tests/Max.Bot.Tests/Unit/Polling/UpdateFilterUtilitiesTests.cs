using FluentAssertions;
using Max.Bot.Configuration;
using Max.Bot.Polling;
using Max.Bot.Types;
using Max.Bot.Types.Enums;
using Xunit;

namespace Max.Bot.Tests.Unit.Polling;

public class UpdateFilterUtilitiesTests
{
    [Fact]
    public void ShouldDispatch_WhenNoFiltersConfigured()
    {
        // Arrange
        var options = CreateOptions();
        var typeFilter = UpdateFilterUtilities.BuildTypeFilter(options);
        var userFilter = UpdateFilterUtilities.BuildAllowedUsernames(options);
        var update = new Update { UpdateTypeRaw = "message_created", Message = new Message { From = new User { Username = "tester" } } };

        // Act
        var result = UpdateFilterUtilities.ShouldDispatch(update, typeFilter, userFilter);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ShouldDispatch_ReturnsFalse_WhenTypeFilteredOut()
    {
        // Arrange
        var options = CreateOptions();
        options.Handling.AllowedUpdateTypes.Add(UpdateType.CallbackQuery);
        var typeFilter = UpdateFilterUtilities.BuildTypeFilter(options);
        var update = new Update { UpdateTypeRaw = "message_created", Message = new Message { From = new User { Username = "tester" } } };

        // Act
        var result = UpdateFilterUtilities.ShouldDispatch(update, typeFilter, null);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldDispatch_ReturnsFalse_WhenUsernameNotAllowed()
    {
        // Arrange
        var options = CreateOptions();
        options.Handling.AllowedUsernames.Add("allowedUser");
        var userFilter = UpdateFilterUtilities.BuildAllowedUsernames(options);
        var update = new Update { UpdateTypeRaw = "message_created", Message = new Message { From = new User { Username = "otherUser" } } };

        // Act
        var result = UpdateFilterUtilities.ShouldDispatch(update, null, userFilter);

        // Assert
        result.Should().BeFalse();
    }

    private static MaxBotOptions CreateOptions()
    {
        return new MaxBotOptions
        {
            Token = "token",
            BaseUrl = "https://api.max.ru/bot"
        };
    }
}




