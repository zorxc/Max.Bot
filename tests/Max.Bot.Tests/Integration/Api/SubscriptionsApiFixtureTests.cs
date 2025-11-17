using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Max.Bot.Api;
using Max.Bot.Tests.Integration.Fixtures;
using Max.Bot.Types.Enums;
using Max.Bot.Types.Requests;

namespace Max.Bot.Tests.Integration.Api;

/// <summary>
/// Exercises <see cref="SubscriptionsApi"/> against deterministic fixtures inspired by VkNet HTML snapshots.
/// </summary>
public class SubscriptionsApiFixtureTests
{
    [Fact]
    public async Task GetSubscriptionsAsync_ShouldReplayFixture()
    {
        var api = FixtureHttpClientFactory.CreateSubscriptionsApi(
            "subscriptions/get_subscriptions_success.json",
            HttpMethod.Get,
            "/bot/subscriptions");

        var subscriptions = await api.GetSubscriptionsAsync();

        subscriptions.Should().HaveCount(1);
        subscriptions[0].Url.Should().Be("https://bot.example/webhook");
    }

    [Fact]
    public async Task GetUpdatesAsync_ShouldRespectQueryParameters()
    {
        var api = FixtureHttpClientFactory.CreateSubscriptionsApi(
            "updates/get_updates_success.json",
            HttpMethod.Get,
            "/bot/updates?limit=25&timeout=30&marker=9001&types=Message%2CCallbackQuery");

        var request = new GetUpdatesRequest
        {
            Limit = 25,
            Timeout = 30,
            Marker = 9001,
            Types = new List<string> { UpdateType.Message.ToString(), UpdateType.CallbackQuery.ToString() }
        };

        var response = await api.GetUpdatesAsync(request);

        response.Marker.Should().Be(9002);
        response.Updates.Should().ContainSingle();

        var update = response.Updates[0];
        update.UpdateId.Should().Be(128);
        update.Type.Should().Be(UpdateType.Message);
        update.Message.Should().NotBeNull();
        update.Message!.Text.Should().Be("Fixtures keep bots honest");
        update.Message.Chat!.Title.Should().Be("QA Squad");
        update.Message.Chat.Id.Should().Be(4242);
    }
}


