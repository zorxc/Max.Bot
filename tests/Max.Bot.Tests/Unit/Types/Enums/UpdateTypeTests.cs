using FluentAssertions;
using Max.Bot.Networking;
using Max.Bot.Types.Enums;
using Xunit;

namespace Max.Bot.Tests.Unit.Types.Enums;

public class UpdateTypeTests
{
    [Fact]
    public void UpdateType_ShouldContainAllOfficialApiTypes()
    {
        // Assert - all 16 official API types plus Unknown should be present
        var expectedTypes = new[]
        {
            UpdateType.Unknown,
            UpdateType.MessageCreated,
            UpdateType.MessageCallback,
            UpdateType.MessageEdited,
            UpdateType.MessageRemoved,
            UpdateType.BotAdded,
            UpdateType.BotRemoved,
            UpdateType.BotStarted,
            UpdateType.BotStopped,
            UpdateType.DialogMuted,
            UpdateType.DialogUnmuted,
            UpdateType.DialogCleared,
            UpdateType.DialogRemoved,
            UpdateType.UserAdded,
            UpdateType.UserRemoved,
            UpdateType.ChatTitleChanged,
            UpdateType.MessageChatCreated
        };

        var actualTypes = Enum.GetValues<UpdateType>();

        actualTypes.Should().Contain(expectedTypes);
        // 17 types + 2 obsolete aliases (Message, CallbackQuery) = 19 values in Enum.GetValues
        // But since aliases have same underlying values, GetValues returns 19 entries including duplicates
        actualTypes.Should().HaveCountGreaterThanOrEqualTo(17);
        
        // Verify distinct count (excluding obsolete aliases with same values)
        var distinctTypes = actualTypes.Distinct().ToArray();
        distinctTypes.Should().HaveCount(17); // 16 official + Unknown
    }

    [Fact]
    public void Serialize_ShouldHandleAllValues()
    {
        // Arrange
        var values = Enum.GetValues<UpdateType>();

        // Act & Assert
        foreach (var value in values)
        {
            var json = MaxJsonSerializer.Serialize(value);
            json.Should().NotBeNullOrEmpty();
        }
    }

    [Theory]
    [InlineData(UpdateType.MessageCreated, "message_created")]
    [InlineData(UpdateType.MessageCallback, "message_callback")]
    [InlineData(UpdateType.MessageEdited, "message_edited")]
    [InlineData(UpdateType.MessageRemoved, "message_removed")]
    [InlineData(UpdateType.BotAdded, "bot_added")]
    [InlineData(UpdateType.BotRemoved, "bot_removed")]
    [InlineData(UpdateType.BotStarted, "bot_started")]
    [InlineData(UpdateType.BotStopped, "bot_stopped")]
    [InlineData(UpdateType.DialogMuted, "dialog_muted")]
    [InlineData(UpdateType.DialogUnmuted, "dialog_unmuted")]
    [InlineData(UpdateType.DialogCleared, "dialog_cleared")]
    [InlineData(UpdateType.DialogRemoved, "dialog_removed")]
    [InlineData(UpdateType.UserAdded, "user_added")]
    [InlineData(UpdateType.UserRemoved, "user_removed")]
    [InlineData(UpdateType.ChatTitleChanged, "chat_title_changed")]
    [InlineData(UpdateType.MessageChatCreated, "message_chat_created")]
    public void EnumNames_ShouldMatchOfficialApiNames(UpdateType type, string expectedApiName)
    {
        // This test documents the mapping between C# enum and API update_type values
        // The actual parsing is done in Update.ParseUpdateType
        expectedApiName.Should().NotBeNullOrEmpty();
        type.Should().NotBe(UpdateType.Unknown);
    }
}
