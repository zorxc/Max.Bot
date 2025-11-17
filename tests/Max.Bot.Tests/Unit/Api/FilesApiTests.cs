using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Max.Bot.Api;
using Max.Bot.Configuration;
using Max.Bot.Exceptions;
using Max.Bot.Networking;
using Max.Bot.Types;
using Max.Bot.Types.Enums;
using Moq;
using Xunit;

namespace Max.Bot.Tests.Unit.Api;

public class FilesApiTests
{
    private readonly Mock<IMaxHttpClient> _mockHttpClient;
    private readonly MaxBotOptions _options;

    public FilesApiTests()
    {
        _mockHttpClient = new Mock<IMaxHttpClient>();
        _options = new MaxBotOptions
        {
            Token = "test-token-123",
            BaseUrl = "https://api.max.ru/bot"
        };
    }

    [Fact]
    public async Task UploadFileAsync_ShouldReturnUploadResponse_WhenRequestSucceeds()
    {
        // Arrange
        var uploadType = UploadType.Video;
        var expectedResponse = new UploadResponse
        {
            Url = "https://vu.mycdn.me/upload.do...",
            Token = "token-123"
        };

        var wrappedResponse = new Response<UploadResponse>
        {
            Ok = true,
            Result = expectedResponse
        };

        _mockHttpClient
            .Setup(x => x.SendAsync<Response<UploadResponse>>(
                It.Is<MaxApiRequest>(req =>
                    req.Method == HttpMethod.Post &&
                    req.Endpoint == "/uploads" &&
                    req.QueryParameters != null &&
                    req.QueryParameters.ContainsKey("type") &&
                    req.QueryParameters["type"] == "video"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(wrappedResponse);

        var filesApi = new FilesApi(_mockHttpClient.Object, _options);

        // Act
        var result = await filesApi.UploadFileAsync(uploadType);

        // Assert
        result.Should().NotBeNull();
        result.Url.Should().Be(expectedResponse.Url);
        result.Token.Should().Be(expectedResponse.Token);
    }

    [Theory]
    [InlineData(UploadType.Image, "image")]
    [InlineData(UploadType.Video, "video")]
    [InlineData(UploadType.Audio, "audio")]
    [InlineData(UploadType.File, "file")]
    public async Task UploadFileAsync_ShouldMapEnumCorrectly_ForAllTypes(UploadType uploadType, string expectedTypeString)
    {
        // Arrange
        var expectedResponse = new UploadResponse
        {
            Url = "https://vu.mycdn.me/upload.do...",
            Token = null
        };

        var wrappedResponse = new Response<UploadResponse>
        {
            Ok = true,
            Result = expectedResponse
        };

        _mockHttpClient
            .Setup(x => x.SendAsync<Response<UploadResponse>>(
                It.Is<MaxApiRequest>(req =>
                    req.QueryParameters != null &&
                    req.QueryParameters.ContainsKey("type") &&
                    req.QueryParameters["type"] == expectedTypeString),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(wrappedResponse);

        var filesApi = new FilesApi(_mockHttpClient.Object, _options);

        // Act
        var result = await filesApi.UploadFileAsync(uploadType);

        // Assert
        result.Should().NotBeNull();
        result.Url.Should().Be(expectedResponse.Url);
    }

    [Fact]
    public async Task UploadFileAsync_ShouldHandleNullToken_WhenTokenIsNotReturned()
    {
        // Arrange
        var uploadType = UploadType.File;
        var expectedResponse = new UploadResponse
        {
            Url = "https://vu.mycdn.me/upload.do...",
            Token = null
        };

        var wrappedResponse = new Response<UploadResponse>
        {
            Ok = true,
            Result = expectedResponse
        };

        _mockHttpClient
            .Setup(x => x.SendAsync<Response<UploadResponse>>(
                It.IsAny<MaxApiRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(wrappedResponse);

        var filesApi = new FilesApi(_mockHttpClient.Object, _options);

        // Act
        var result = await filesApi.UploadFileAsync(uploadType);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().BeNull();
    }

    [Fact]
    public async Task UploadFileDataAsync_ShouldThrowArgumentException_WhenUploadUrlIsNullOrEmpty()
    {
        // Arrange
        var filesApi = new FilesApi(_mockHttpClient.Object, _options);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("test content"));

        // Act
        var act = async () => await filesApi.UploadFileDataAsync(null!, stream);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("uploadUrl");
    }

    [Fact]
    public async Task UploadFileDataAsync_ShouldThrowArgumentNullException_WhenFileStreamIsNull()
    {
        // Arrange
        var filesApi = new FilesApi(_mockHttpClient.Object, _options);

        // Act
        var act = async () => await filesApi.UploadFileDataAsync("https://example.com/upload", null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("fileStream");
    }

    [Fact]
    public async Task UploadFileResumableAsync_ShouldThrowArgumentException_WhenUploadUrlIsNullOrEmpty()
    {
        // Arrange
        var filesApi = new FilesApi(_mockHttpClient.Object, _options);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("test content"));

        // Act
        var act = async () => await filesApi.UploadFileResumableAsync(null!, stream);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("uploadUrl");
    }

    [Fact]
    public async Task UploadFileResumableAsync_ShouldThrowArgumentNullException_WhenFileStreamIsNull()
    {
        // Arrange
        var filesApi = new FilesApi(_mockHttpClient.Object, _options);

        // Act
        var act = async () => await filesApi.UploadFileResumableAsync("https://example.com/upload", null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("fileStream");
    }

    [Fact]
    public async Task UploadFileResumableAsync_ShouldThrowArgumentException_WhenChunkSizeIsZero()
    {
        // Arrange
        var filesApi = new FilesApi(_mockHttpClient.Object, _options);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("test content"));

        // Act
        var act = async () => await filesApi.UploadFileResumableAsync("https://example.com/upload", stream, chunkSize: 0);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("chunkSize");
    }

    [Fact]
    public async Task UploadFileResumableAsync_ShouldThrowArgumentException_WhenChunkSizeIsNegative()
    {
        // Arrange
        var filesApi = new FilesApi(_mockHttpClient.Object, _options);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("test content"));

        // Act
        var act = async () => await filesApi.UploadFileResumableAsync("https://example.com/upload", stream, chunkSize: -1);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("chunkSize");
    }
}

