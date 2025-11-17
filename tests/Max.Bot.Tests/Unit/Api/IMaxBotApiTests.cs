using FluentAssertions;
using Max.Bot.Api;
using Xunit;

namespace Max.Bot.Tests.Unit.Api;

public class IMaxBotApiTests
{
    [Fact]
    public void IMaxBotApi_ShouldHaveBotProperty()
    {
        // Assert
        typeof(IMaxBotApi).Should().HaveProperty<IBotApi>("Bot");
    }

    [Fact]
    public void IMaxBotApi_ShouldHaveMessagesProperty()
    {
        // Assert
        typeof(IMaxBotApi).Should().HaveProperty<IMessagesApi>("Messages");
    }

    [Fact]
    public void IMaxBotApi_ShouldHaveChatsProperty()
    {
        // Assert
        typeof(IMaxBotApi).Should().HaveProperty<IChatsApi>("Chats");
    }

    [Fact]
    public void IMaxBotApi_ShouldHaveUsersProperty()
    {
        // Assert
        typeof(IMaxBotApi).Should().HaveProperty<IUsersApi>("Users");
    }

    [Fact]
    public void IMaxBotApi_ShouldHaveFilesProperty()
    {
        // Assert
        typeof(IMaxBotApi).Should().HaveProperty<IFilesApi>("Files");
    }

    [Fact]
    public void IMaxBotApi_ShouldHaveSubscriptionsProperty()
    {
        // Assert
        typeof(IMaxBotApi).Should().HaveProperty<ISubscriptionsApi>("Subscriptions");
    }

    [Fact]
    public void IMaxBotApi_ShouldHaveAllRequiredProperties()
    {
        // Act
        var properties = typeof(IMaxBotApi).GetProperties();

        // Assert
        properties.Should().HaveCount(6);
        properties.Should().Contain(p => p.Name == "Bot" && p.PropertyType == typeof(IBotApi));
        properties.Should().Contain(p => p.Name == "Messages" && p.PropertyType == typeof(IMessagesApi));
        properties.Should().Contain(p => p.Name == "Chats" && p.PropertyType == typeof(IChatsApi));
        properties.Should().Contain(p => p.Name == "Users" && p.PropertyType == typeof(IUsersApi));
        properties.Should().Contain(p => p.Name == "Files" && p.PropertyType == typeof(IFilesApi));
        properties.Should().Contain(p => p.Name == "Subscriptions" && p.PropertyType == typeof(ISubscriptionsApi));
    }
}

