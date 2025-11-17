using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using FluentAssertions;
using Max.Bot.Api;
using Max.Bot.Configuration;
using Max.Bot.Networking;

namespace Max.Bot.Tests.Integration.Fixtures;

/// <summary>
/// Factory helpers for spinning up API clients with deterministic HTTP fixtures.
/// </summary>
internal static class FixtureHttpClientFactory
{
    /// <summary>
    /// Creates a <see cref="SubscriptionsApi"/> instance backed by a fixture file.
    /// </summary>
    /// <param name="fixtureFile">Relative path under Integration/Fixtures.</param>
    /// <param name="method">Expected HTTP method.</param>
    /// <param name="expectedPath">Expected path + query (e.g., /TOKEN/updates?limit=1).</param>
    /// <param name="assertRequest">Optional callback for extra request validation.</param>
    /// <returns>A configured <see cref="SubscriptionsApi"/> using fixture data.</returns>
    public static SubscriptionsApi CreateSubscriptionsApi(
        string fixtureFile,
        HttpMethod method,
        string expectedPath,
        Action<HttpRequestMessage>? assertRequest = null)
    {
        var handler = new FixtureHttpMessageHandler(fixtureFile, method, expectedPath, assertRequest);
        var httpClient = new HttpClient(handler);
        var clientOptions = new MaxBotClientOptions
        {
            BaseUrl = "https://api.max.ru/bot",
            Timeout = TimeSpan.FromSeconds(5),
            RetryCount = 0,
            RetryBaseDelay = TimeSpan.FromMilliseconds(10),
            MaxRetryDelay = TimeSpan.FromMilliseconds(10),
            EnableDetailedLogging = false
        };

        var maxHttpClient = new MaxHttpClient(httpClient, clientOptions);
        var botOptions = new MaxBotOptions
        {
            Token = "TEST_TOKEN"
        };

        return new SubscriptionsApi(maxHttpClient, botOptions);
    }

    private sealed class FixtureHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpMethod _expectedMethod;
        private readonly string _expectedPath;
        private readonly string _payload;
        private readonly Action<HttpRequestMessage>? _assertRequest;

        public FixtureHttpMessageHandler(
            string fixtureFile,
            HttpMethod expectedMethod,
            string expectedPath,
            Action<HttpRequestMessage>? assertRequest)
        {
            _expectedMethod = expectedMethod;
            _expectedPath = expectedPath;
            _assertRequest = assertRequest;

            var fullPath = ResolveFixturePath(fixtureFile);
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"Fixture '{fixtureFile}' was not found.", fullPath);
            }

            _payload = File.ReadAllText(fullPath);
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Should().NotBeNull();
            request.Method.Should().Be(_expectedMethod);
            request.RequestUri.Should().NotBeNull();
            request.RequestUri!.PathAndQuery.Should().Be(_expectedPath);

            _assertRequest?.Invoke(request);

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(_payload, Encoding.UTF8, "application/json")
            };

            return Task.FromResult(response);
        }

        private static string ResolveFixturePath(string relativePath)
        {
            var baseDirectory = AppContext.BaseDirectory;
            var fixtureDirectory = Path.Combine(baseDirectory, "Integration", "Fixtures");
            return Path.Combine(fixtureDirectory, relativePath.Replace('/', Path.DirectorySeparatorChar));
        }
    }
}


