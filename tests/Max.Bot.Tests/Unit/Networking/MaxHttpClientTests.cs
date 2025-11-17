using System.Net;
using System.Net.Http;
using System.Text;
using FluentAssertions;
using Max.Bot.Configuration;
using Max.Bot.Exceptions;
using Max.Bot.Networking;
using Xunit;

namespace Max.Bot.Tests.Unit.Networking;

public class MaxHttpClientTests
{
    private readonly MaxBotClientOptions _defaultOptions;

    public MaxHttpClientTests()
    {
        _defaultOptions = new MaxBotClientOptions
        {
            BaseUrl = "https://api.max.ru/bot",
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenHttpClientIsNull()
    {
        // Act
        var act = () => new MaxHttpClient(null!, _defaultOptions);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("httpClient");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenOptionsIsNull()
    {
        // Arrange
        var httpClient = new HttpClient();

        // Act
        var act = () => new MaxHttpClient(httpClient, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("options");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenOptionsAreInvalid()
    {
        // Arrange
        var httpClient = new HttpClient();
        var invalidOptions = new MaxBotClientOptions { BaseUrl = string.Empty };

        // Act
        var act = () => new MaxHttpClient(httpClient, invalidOptions);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_ShouldConfigureHttpClient()
    {
        // Arrange
        var httpClient = new HttpClient();

        // Act
        var client = new MaxHttpClient(httpClient, _defaultOptions);

        // Assert
        httpClient.BaseAddress.Should().Be(new Uri("https://api.max.ru/bot"));
        httpClient.Timeout.Should().Be(TimeSpan.FromSeconds(30));
        httpClient.DefaultRequestHeaders.Accept.Should().Contain(h => h.MediaType == "application/json");
    }

    [Fact]
    public async Task SendAsync_Generic_ShouldReturnDeserializedResponse()
    {
        // Arrange
        var handler = new TestHttpMessageHandler
        {
            Response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"id\":123,\"name\":\"Test\"}", Encoding.UTF8, "application/json")
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
        var result = await client.SendAsync<TestResponse>(request);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(123);
        result.Name.Should().Be("Test");
        handler.Requests.Should().HaveCount(1);
        handler.Requests[0].RequestUri!.AbsoluteUri.Should().Be("https://api.max.ru/bot/test");
    }

    [Fact]
    public async Task SendAsync_NonGeneric_ShouldSucceed()
    {
        // Arrange
        var handler = new TestHttpMessageHandler
        {
            Response = new HttpResponseMessage(HttpStatusCode.OK)
        };
        var httpClient = new HttpClient(handler);
        var client = new MaxHttpClient(httpClient, _defaultOptions);
        var request = new MaxApiRequest
        {
            Method = HttpMethod.Get,
            Endpoint = "test"
        };

        // Act
        var act = async () => await client.SendAsync(request);

        // Assert
        await act.Should().NotThrowAsync();
        handler.Requests.Should().HaveCount(1);
    }

    [Fact]
    public async Task SendAsync_ShouldIncludeQueryParameters()
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
            Method = HttpMethod.Get,
            Endpoint = "test",
            QueryParameters = new Dictionary<string, string?>
            {
                { "param1", "value1" },
                { "param2", "value2" }
            }
        };

        // Act
        await client.SendAsync<object>(request);

        // Assert
        handler.Requests[0].RequestUri!.Query.Should().Contain("param1=value1");
        handler.Requests[0].RequestUri!.Query.Should().Contain("param2=value2");
    }

    [Fact]
    public async Task SendAsync_ShouldIncludeHeaders()
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
            Method = HttpMethod.Get,
            Endpoint = "test",
            Headers = new Dictionary<string, string?>
            {
                { "X-Custom-Header", "custom-value" }
            }
        };

        // Act
        await client.SendAsync<object>(request);

        // Assert
        handler.Requests[0].Headers.Should().Contain(h => h.Key == "X-Custom-Header");
    }

    [Fact]
    public async Task SendAsync_ShouldSerializeRequestBody()
    {
        // Arrange
        string? requestContent = null;
        var handler = new TestHttpMessageHandler
        {
            Response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            },
            OnRequest = (req) =>
            {
                if (req.Content != null)
                {
                    requestContent = req.Content.ReadAsStringAsync().Result;
                }
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
        await client.SendAsync<object>(request);

        // Assert
        handler.Requests.Should().HaveCount(1);
        requestContent.Should().NotBeNull();
        requestContent.Should().Contain("Test");
        requestContent.Should().Contain("123");
        handler.Requests[0].Content!.Headers.ContentType!.MediaType.Should().Be("application/json");
    }

    [Fact]
    public async Task SendAsync_ShouldThrowMaxUnauthorizedException_WhenStatusCodeIs401()
    {
        // Arrange
        var handler = new TestHttpMessageHandler
        {
            Response = new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {
                Content = new StringContent("Unauthorized", Encoding.UTF8, "text/plain")
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
        var act = async () => await client.SendAsync<object>(request);

        // Assert
        await act.Should().ThrowAsync<MaxUnauthorizedException>();
    }

    [Fact]
    public async Task SendAsync_ShouldThrowMaxUnauthorizedException_WhenStatusCodeIs403()
    {
        // Arrange
        var handler = new TestHttpMessageHandler
        {
            Response = new HttpResponseMessage(HttpStatusCode.Forbidden)
            {
                Content = new StringContent("Forbidden", Encoding.UTF8, "text/plain")
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
        var act = async () => await client.SendAsync<object>(request);

        // Assert
        await act.Should().ThrowAsync<MaxUnauthorizedException>();
    }

    [Fact]
    public async Task SendAsync_ShouldThrowMaxRateLimitException_WhenStatusCodeIs429()
    {
        // Arrange
        var handler = new TestHttpMessageHandler
        {
            Response = new HttpResponseMessage(HttpStatusCode.TooManyRequests)
            {
                Content = new StringContent("Too Many Requests", Encoding.UTF8, "text/plain"),
                Headers = { { "Retry-After", "60" } }
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
        var act = async () => await client.SendAsync<object>(request);

        // Assert
        var exception = await act.Should().ThrowAsync<MaxRateLimitException>();
        exception.Which.RetryAfter.Should().Be(TimeSpan.FromSeconds(60));
    }

    [Fact]
    public async Task SendAsync_ShouldThrowMaxNetworkException_WhenStatusCodeIs500()
    {
        // Arrange
        var handler = new TestHttpMessageHandler
        {
            Response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("Internal Server Error", Encoding.UTF8, "text/plain")
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
        var act = async () => await client.SendAsync<object>(request);

        // Assert
        await act.Should().ThrowAsync<MaxNetworkException>();
    }

    [Fact]
    public async Task SendAsync_ShouldThrowMaxNetworkException_WhenStatusCodeIsTimeout()
    {
        // Arrange
        var handler = new TestHttpMessageHandler
        {
            Response = new HttpResponseMessage(HttpStatusCode.RequestTimeout)
            {
                Content = new StringContent("Request Timeout", Encoding.UTF8, "text/plain")
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
        var act = async () => await client.SendAsync<object>(request);

        // Assert
        await act.Should().ThrowAsync<MaxNetworkException>();
    }

    [Fact]
    public async Task SendAsync_ShouldThrowMaxApiException_WhenStatusCodeIs400()
    {
        // Arrange
        var handler = new TestHttpMessageHandler
        {
            Response = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("Bad Request", Encoding.UTF8, "text/plain")
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
        var act = async () => await client.SendAsync<object>(request);

        // Assert
        await act.Should().ThrowAsync<MaxApiException>();
    }

    [Fact]
    public async Task SendAsync_ShouldThrowJsonException_WhenResponseContentIsNull()
    {
        // Arrange
        var handler = new TestHttpMessageHandler
        {
            Response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = null
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
        var act = async () => await client.SendAsync<object>(request);

        // Assert
        // HttpClient returns empty stream when Content is null, which causes JsonException
        // MaxHttpClient wraps JsonException in MaxNetworkException
        // This test verifies that we handle the error appropriately
        await act.Should().ThrowAsync<Max.Bot.Exceptions.MaxNetworkException>()
            .Where(ex => ex.InnerException is System.Text.Json.JsonException);
    }

    [Fact]
    public async Task SendAsync_ShouldThrowArgumentNullException_WhenRequestIsNull()
    {
        // Arrange
        var httpClient = new HttpClient();
        var client = new MaxHttpClient(httpClient, _defaultOptions);

        // Act
        var act = async () => await client.SendAsync<object>(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task SendAsync_ShouldDeserializeErrorResponse_WhenApiReturnsError()
    {
        // Arrange
        var errorResponseJson = """{"ok":false,"error":{"code":"INVALID_TOKEN","message":"Bot token is invalid","details":{"param":"token"}}}""";
        var handler = new TestHttpMessageHandler
        {
            Response = new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {
                Content = new StringContent(errorResponseJson, Encoding.UTF8, "application/json")
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
        var act = async () => await client.SendAsync<object>(request);

        // Assert
        var exception = await act.Should().ThrowAsync<MaxUnauthorizedException>();
        exception.Which.Message.Should().Contain("Bot token is invalid");
        exception.Which.ErrorCode.Should().Be("INVALID_TOKEN");
    }

    [Fact]
    public async Task SendAsync_ShouldUseRawBody_WhenErrorResponseDeserializationFails()
    {
        // Arrange
        var invalidJson = "not valid json";
        var handler = new TestHttpMessageHandler
        {
            Response = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(invalidJson, Encoding.UTF8, "application/json")
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
        var act = async () => await client.SendAsync<object>(request);

        // Assert
        var exception = await act.Should().ThrowAsync<MaxApiException>();
        exception.Which.Message.Should().Contain("not valid json");
        exception.Which.ErrorCode.Should().BeNull();
    }

    [Fact]
    public async Task SendAsync_ShouldUseDefaultMessage_WhenErrorResponseBodyIsNull()
    {
        // Arrange
        var handler = new TestHttpMessageHandler
        {
            Response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = null
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
        var act = async () => await client.SendAsync<object>(request);

        // Assert
        var exception = await act.Should().ThrowAsync<MaxNetworkException>();
        exception.Which.Message.Should().Contain("HTTP 500");
        exception.Which.ErrorCode.Should().BeNull();
    }

    [Fact]
    public async Task SendAsync_ShouldExtractErrorCode_WhenErrorResponseHasCode()
    {
        // Arrange
        var errorResponseJson = """{"ok":false,"error":{"code":"RATE_LIMIT_EXCEEDED","message":"Too many requests"}}""";
        var handler = new TestHttpMessageHandler
        {
            Response = new HttpResponseMessage(HttpStatusCode.TooManyRequests)
            {
                Content = new StringContent(errorResponseJson, Encoding.UTF8, "application/json"),
                Headers = { { "Retry-After", "60" } }
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
        var act = async () => await client.SendAsync<object>(request);

        // Assert
        var exception = await act.Should().ThrowAsync<MaxRateLimitException>();
        exception.Which.Message.Should().Contain("Too many requests");
        exception.Which.ErrorCode.Should().Be("RATE_LIMIT_EXCEEDED");
        exception.Which.RetryAfter.Should().Be(TimeSpan.FromSeconds(60));
    }

    private class TestHttpMessageHandler : HttpMessageHandler
    {
        public HttpResponseMessage Response { get; set; } = new(HttpStatusCode.OK);
        public List<HttpRequestMessage> Requests { get; } = new();
        public Action<HttpRequestMessage>? OnRequest { get; set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Requests.Add(request);
            OnRequest?.Invoke(request);

            // Read content from current Response (not cached) to support dynamic Response changes
            string? content = null;
            string? contentType = null;

            if (Response.Content != null)
            {
                try
                {
                    content = Response.Content.ReadAsStringAsync(cancellationToken).GetAwaiter().GetResult();
                    contentType = Response.Content.Headers.ContentType?.MediaType ?? "application/json";
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
            var response = new HttpResponseMessage(Response.StatusCode)
            {
                Content = content != null
                    ? new StringContent(content, Encoding.UTF8, contentType ?? "application/json")
                    : null
            };

            // Copy headers
            foreach (var header in Response.Headers)
            {
                response.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            return Task.FromResult(response);
        }
    }

    private class TestResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}

