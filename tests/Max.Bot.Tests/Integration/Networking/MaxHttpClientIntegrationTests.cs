using System.Net;
using System.Net.Http;
using System.Text;
using FluentAssertions;
using Max.Bot.Configuration;
using Max.Bot.Exceptions;
using Max.Bot.Networking;
using Xunit;

namespace Max.Bot.Tests.Integration.Networking;

public class MaxHttpClientIntegrationTests
{
    private readonly MaxBotClientOptions _defaultOptions;

    public MaxHttpClientIntegrationTests()
    {
        _defaultOptions = new MaxBotClientOptions
        {
            BaseUrl = "https://api.max.ru/bot",
            Timeout = TimeSpan.FromSeconds(30),
            RetryCount = 3
        };
    }

    [Fact]
    public async Task SendAsync_ShouldCompleteFullCycle_WithSuccessfulRequest()
    {
        // Arrange
        var handler = new TestHttpMessageHandler
        {
            Response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"result\":\"success\",\"data\":{\"id\":123}}", Encoding.UTF8, "application/json")
            }
        };
        var httpClient = new HttpClient(handler);
        var client = new MaxHttpClient(httpClient, _defaultOptions);
        var request = new MaxApiRequest
        {
            Method = HttpMethod.Post,
            Endpoint = "test",
            Body = new { name = "Test", value = 123 }
        };

        // Act
        var result = await client.SendAsync<TestResponse>(request);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().Be("success");
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(123);
        handler.Requests.Should().HaveCount(1);
        handler.Requests[0].Method.Should().Be(HttpMethod.Post);
        handler.Requests[0].RequestUri!.ToString().Should().Contain("test");
    }

    [Fact]
    public async Task SendAsync_ShouldHandleDifferentHttpStatusCodes()
    {
        // Arrange
        var testCases = new[]
        {
            (StatusCode: HttpStatusCode.Unauthorized, ExpectedException: typeof(MaxUnauthorizedException)),
            (StatusCode: HttpStatusCode.Forbidden, ExpectedException: typeof(MaxUnauthorizedException)),
            (StatusCode: HttpStatusCode.TooManyRequests, ExpectedException: typeof(MaxRateLimitException)),
            (StatusCode: HttpStatusCode.InternalServerError, ExpectedException: typeof(MaxNetworkException)),
            (StatusCode: HttpStatusCode.ServiceUnavailable, ExpectedException: typeof(MaxNetworkException)),
            (StatusCode: HttpStatusCode.BadRequest, ExpectedException: typeof(MaxApiException))
        };

        foreach (var testCase in testCases)
        {
            var handler = new TestHttpMessageHandler
            {
                Response = new HttpResponseMessage(testCase.StatusCode)
                {
                    Content = new StringContent("Error", Encoding.UTF8, "text/plain")
                }
            };
            var httpClient = new HttpClient(handler);
            var client = new MaxHttpClient(httpClient, _defaultOptions);
            var request = new MaxApiRequest
            {
                Method = HttpMethod.Get,
                Endpoint = "test"
            };

            // Act
            var act = async () => await client.SendAsync<TestResponse>(request);

            // Assert
            if (testCase.ExpectedException == typeof(MaxUnauthorizedException))
            {
                await act.Should().ThrowAsync<MaxUnauthorizedException>();
            }
            else if (testCase.ExpectedException == typeof(MaxRateLimitException))
            {
                await act.Should().ThrowAsync<MaxRateLimitException>();
            }
            else if (testCase.ExpectedException == typeof(MaxNetworkException))
            {
                await act.Should().ThrowAsync<MaxNetworkException>();
            }
            else if (testCase.ExpectedException == typeof(MaxApiException))
            {
                await act.Should().ThrowAsync<MaxApiException>();
            }
        }
    }

    [Fact]
    public async Task SendAsync_ShouldRetry_OnTransientErrors()
    {
        // Arrange
        var attemptCount = 0;
        var handler = new TestHttpMessageHandler
        {
            Response = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
            {
                Content = new StringContent("Service Unavailable", Encoding.UTF8, "text/plain")
            }
        };
        handler.OnRequest = (req) =>
        {
            attemptCount++;
            if (attemptCount >= 2)
            {
                handler.Response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"result\":\"success\"}", Encoding.UTF8, "application/json")
                };
            }
        };
        var httpClient = new HttpClient(handler);
        var options = new MaxBotClientOptions
        {
            BaseUrl = "https://api.max.ru/bot",
            Timeout = TimeSpan.FromSeconds(30),
            RetryCount = 3,
            RetryBaseDelay = TimeSpan.FromMilliseconds(10) // Fast retry for tests
        };
        var client = new MaxHttpClient(httpClient, options);
        var request = new MaxApiRequest
        {
            Method = HttpMethod.Get,
            Endpoint = "test"
        };

        // Act
        var result = await client.SendAsync<TestResponse>(request);

        // Assert
        result.Should().NotBeNull();
        attemptCount.Should().Be(2);
        handler.Requests.Should().HaveCount(2);
    }

    [Fact]
    public async Task SendAsync_ShouldNotRetry_OnNonRetryableErrors()
    {
        // Arrange
        var attemptCount = 0;
        var handler = new TestHttpMessageHandler
        {
            Response = new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {
                Content = new StringContent("Unauthorized", Encoding.UTF8, "text/plain")
            }
        };
        handler.OnRequest = (req) => attemptCount++;
        var httpClient = new HttpClient(handler);
        var client = new MaxHttpClient(httpClient, _defaultOptions);
        var request = new MaxApiRequest
        {
            Method = HttpMethod.Get,
            Endpoint = "test"
        };

        // Act
        var act = async () => await client.SendAsync<TestResponse>(request);

        // Assert
        await act.Should().ThrowAsync<MaxUnauthorizedException>();
        attemptCount.Should().Be(1);
        handler.Requests.Should().HaveCount(1);
    }

    [Fact]
    public async Task SendAsync_ShouldHandleNonGenericMethod()
    {
        // Arrange
        var handler = new TestHttpMessageHandler
        {
            Response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            }
        };
        var httpClient = new HttpClient(handler);
        var client = new MaxHttpClient(httpClient, _defaultOptions);
        var request = new MaxApiRequest
        {
            Method = HttpMethod.Delete,
            Endpoint = "test"
        };

        // Act
        await client.SendAsync(request);

        // Assert
        handler.Requests.Should().HaveCount(1);
        handler.Requests[0].Method.Should().Be(HttpMethod.Delete);
    }

    [Fact]
    public async Task SendAsync_ShouldIncludeQueryParameters()
    {
        // Arrange
        var handler = new TestHttpMessageHandler
        {
            Response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"result\":\"success\"}", Encoding.UTF8, "application/json")
            }
        };
        var httpClient = new HttpClient(handler);
        var client = new MaxHttpClient(httpClient, _defaultOptions);
        var request = new MaxApiRequest
        {
            Method = HttpMethod.Get,
            Endpoint = "test",
            QueryParameters = new Dictionary<string, string?>
            {
                { "param1", "value1" },
                { "param2", "value2" }
            }
        };

        // Act
        await client.SendAsync<TestResponse>(request);

        // Assert
        handler.Requests.Should().HaveCount(1);
        handler.Requests[0].RequestUri!.Query.Should().Contain("param1=value1");
        handler.Requests[0].RequestUri!.Query.Should().Contain("param2=value2");
    }

    [Fact]
    public async Task SendAsync_ShouldSerializeRequestBody()
    {
        // Arrange
        var handler = new TestHttpMessageHandler
        {
            Response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"result\":\"success\"}", Encoding.UTF8, "application/json")
            }
        };
        var httpClient = new HttpClient(handler);
        var client = new MaxHttpClient(httpClient, _defaultOptions);
        var requestBody = new { name = "Test", value = 123 };
        var request = new MaxApiRequest
        {
            Method = HttpMethod.Post,
            Endpoint = "test",
            Body = requestBody
        };

        // Act
        await client.SendAsync<TestResponse>(request);

        // Assert
        handler.Requests.Should().HaveCount(1);
        var content = handler.GetSavedRequestBody();
        content.Should().NotBeNull();
        content.Should().Contain("Test");
        content.Should().Contain("123");
    }

    private class TestHttpMessageHandler : HttpMessageHandler
    {
        private string? _savedRequestBody;

        public HttpResponseMessage? Response { get; set; }
        public Func<HttpRequestMessage, HttpResponseMessage>? ResponseFactory { get; set; }
        public List<HttpRequestMessage> Requests { get; } = new();
        public Action<HttpRequestMessage>? OnRequest { get; set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Requests.Add(request);

            // Save request body content before it might be disposed
            if (request.Content != null)
            {
                try
                {
                    _savedRequestBody = request.Content.ReadAsStringAsync(cancellationToken).GetAwaiter().GetResult();
                }
                catch
                {
                    // If already disposed, ignore
                }
            }

            OnRequest?.Invoke(request);

            HttpResponseMessage? responseToUse = null;

            if (ResponseFactory != null)
            {
                responseToUse = ResponseFactory(request);
            }
            else if (Response != null)
            {
                responseToUse = Response;
            }

            if (responseToUse == null)
            {
                // Default: simulate network error
                throw new HttpRequestException("Network error");
            }

            // Read content from current responseToUse (not cached) to support dynamic Response changes
            string? content = null;
            string? contentType = null;

            if (responseToUse.Content != null)
            {
                try
                {
                    content = responseToUse.Content.ReadAsStringAsync(cancellationToken).GetAwaiter().GetResult();
                    contentType = responseToUse.Content.Headers.ContentType?.MediaType ?? "application/json";
                }
                catch (ObjectDisposedException)
                {
                    // If content is already disposed, try to create empty content
                    content = string.Empty;
                    contentType = "application/json";
                }
            }

            // Create a new HttpResponseMessage for each request to avoid ObjectDisposedException
            // when content is read multiple times (e.g., during retries)
            var response = new HttpResponseMessage(responseToUse.StatusCode)
            {
                Content = content != null
                    ? new StringContent(content, Encoding.UTF8, contentType ?? "application/json")
                    : null
            };

            // Copy headers
            foreach (var header in responseToUse.Headers)
            {
                response.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            return Task.FromResult(response);
        }

        /// <summary>
        /// Gets the saved request body content (for testing purposes).
        /// </summary>
        public string? GetSavedRequestBody() => _savedRequestBody;
    }

    private class TestResponse
    {
        public string? Result { get; set; }
        public TestData? Data { get; set; }
    }

    private class TestData
    {
        public int Id { get; set; }
    }
}

