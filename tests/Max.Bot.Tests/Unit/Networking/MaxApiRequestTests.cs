using System.Net.Http;
using FluentAssertions;
using Max.Bot.Networking;
using Xunit;

namespace Max.Bot.Tests.Unit.Networking;

public class MaxApiRequestTests
{
    [Fact]
    public void MaxApiRequest_ShouldHaveDefaultValues()
    {
        // Act
        var request = new MaxApiRequest();

        // Assert
        request.Method.Should().Be(HttpMethod.Get);
        request.Endpoint.Should().BeEmpty();
        request.Body.Should().BeNull();
        request.QueryParameters.Should().BeNull();
        request.Headers.Should().BeNull();
    }

    [Fact]
    public void BuildUrl_ShouldCombineBaseUrlAndEndpoint()
    {
        // Arrange
        var request = new MaxApiRequest
        {
            Endpoint = "me"
        };
        var baseUrl = "https://api.max.ru/bot";

        // Act
        var url = request.BuildUrl(baseUrl);

        // Assert
        url.Should().Be("https://api.max.ru/bot/me");
    }

    [Fact]
    public void BuildUrl_ShouldHandleTrailingSlashInBaseUrl()
    {
        // Arrange
        var request = new MaxApiRequest
        {
            Endpoint = "me"
        };
        var baseUrl = "https://api.max.ru/bot/";

        // Act
        var url = request.BuildUrl(baseUrl);

        // Assert
        url.Should().Be("https://api.max.ru/bot/me");
    }

    [Fact]
    public void BuildUrl_ShouldHandleLeadingSlashInEndpoint()
    {
        // Arrange
        var request = new MaxApiRequest
        {
            Endpoint = "/me"
        };
        var baseUrl = "https://api.max.ru/bot";

        // Act
        var url = request.BuildUrl(baseUrl);

        // Assert
        url.Should().Be("https://api.max.ru/bot/me");
    }

    [Fact]
    public void BuildUrl_ShouldHandleBothSlashes()
    {
        // Arrange
        var request = new MaxApiRequest
        {
            Endpoint = "/me"
        };
        var baseUrl = "https://api.max.ru/bot/";

        // Act
        var url = request.BuildUrl(baseUrl);

        // Assert
        url.Should().Be("https://api.max.ru/bot/me");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void BuildUrl_ShouldThrow_WhenBaseUrlIsNullOrEmpty(string? baseUrl)
    {
        // Arrange
        var request = new MaxApiRequest
        {
            Endpoint = "me"
        };

        // Act
        var act = () => request.BuildUrl(baseUrl!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName(nameof(baseUrl));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void BuildUrl_ShouldThrow_WhenEndpointIsNullOrEmpty(string? endpoint)
    {
        // Arrange
        var request = new MaxApiRequest
        {
            Endpoint = endpoint!
        };
        var baseUrl = "https://api.max.ru/bot";

        // Act
        var act = () => request.BuildUrl(baseUrl);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("Endpoint");
    }

    [Fact]
    public void BuildQueryString_ShouldReturnEmptyString_WhenQueryParametersIsNull()
    {
        // Arrange
        var request = new MaxApiRequest
        {
            QueryParameters = null
        };

        // Act
        var queryString = request.BuildQueryString();

        // Assert
        queryString.Should().BeEmpty();
    }

    [Fact]
    public void BuildQueryString_ShouldReturnEmptyString_WhenQueryParametersIsEmpty()
    {
        // Arrange
        var request = new MaxApiRequest
        {
            QueryParameters = new Dictionary<string, string?>()
        };

        // Act
        var queryString = request.BuildQueryString();

        // Assert
        queryString.Should().BeEmpty();
    }

    [Fact]
    public void BuildQueryString_ShouldBuildQueryString_FromQueryParameters()
    {
        // Arrange
        var request = new MaxApiRequest
        {
            QueryParameters = new Dictionary<string, string?>
            {
                { "limit", "10" },
                { "offset", "0" }
            }
        };

        // Act
        var queryString = request.BuildQueryString();

        // Assert
        queryString.Should().Be("limit=10&offset=0");
    }

    [Fact]
    public void BuildQueryString_ShouldSkipNullValues()
    {
        // Arrange
        var request = new MaxApiRequest
        {
            QueryParameters = new Dictionary<string, string?>
            {
                { "limit", "10" },
                { "offset", null },
                { "sort", "date" }
            }
        };

        // Act
        var queryString = request.BuildQueryString();

        // Assert
        queryString.Should().Be("limit=10&sort=date");
    }

    [Fact]
    public void BuildQueryString_ShouldEncodeUrlParameters()
    {
        // Arrange
        var request = new MaxApiRequest
        {
            QueryParameters = new Dictionary<string, string?>
            {
                { "query", "hello world" },
                { "filter", "test&value" }
            }
        };

        // Act
        var queryString = request.BuildQueryString();

        // Assert
        queryString.Should().Be("query=hello%20world&filter=test%26value");
    }

    [Fact]
    public void BuildUrl_ShouldAppendQueryString()
    {
        // Arrange
        var request = new MaxApiRequest
        {
            Endpoint = "messages",
            QueryParameters = new Dictionary<string, string?>
            {
                { "limit", "10" },
                { "offset", "0" }
            }
        };
        var baseUrl = "https://api.max.ru/bot";

        // Act
        var url = request.BuildUrl(baseUrl);

        // Assert
        url.Should().Be("https://api.max.ru/bot/messages?limit=10&offset=0");
    }

    [Fact]
    public void BuildUrl_ShouldHandleSpecialCharactersInQueryParameters()
    {
        // Arrange
        var request = new MaxApiRequest
        {
            Endpoint = "search",
            QueryParameters = new Dictionary<string, string?>
            {
                { "q", "hello+world" },
                { "filter", "test value" }
            }
        };
        var baseUrl = "https://api.max.ru/bot";

        // Act
        var url = request.BuildUrl(baseUrl);

        // Assert
        url.Should().Contain("q=hello%2Bworld");
        url.Should().Contain("filter=test%20value");
    }

    [Fact]
    public void MaxApiRequest_ShouldSupportDifferentHttpMethods()
    {
        // Arrange & Act
        var getRequest = new MaxApiRequest { Method = HttpMethod.Get };
        var postRequest = new MaxApiRequest { Method = HttpMethod.Post };
        var putRequest = new MaxApiRequest { Method = HttpMethod.Put };
        var deleteRequest = new MaxApiRequest { Method = HttpMethod.Delete };

        // Assert
        getRequest.Method.Should().Be(HttpMethod.Get);
        postRequest.Method.Should().Be(HttpMethod.Post);
        putRequest.Method.Should().Be(HttpMethod.Put);
        deleteRequest.Method.Should().Be(HttpMethod.Delete);
    }
}

