// 📁 [DocumentTests] - Тесты для модели Document
// 🎯 Core function: Тестирование сериализации/десериализации Document
// 🔗 Key dependencies: Max.Bot.Types, Max.Bot.Networking, FluentAssertions, xUnit
// 💡 Usage: Unit тесты для проверки корректности работы модели Document

using FluentAssertions;
using Max.Bot.Networking;
using Max.Bot.Types;
using Xunit;

namespace Max.Bot.Tests.Unit.Types;

public class DocumentTests
{
    [Fact]
    public void Document_ShouldDeserialize_FromJson()
    {
        // Arrange
        var json = """{"id":123,"fileId":"doc123","fileName":"document.pdf","fileSize":2097152,"mimeType":"application/pdf","url":"https://example.com/document.pdf"}""";

        // Act
        var document = MaxJsonSerializer.Deserialize<Document>(json);

        // Assert
        document.Should().NotBeNull();
        document.Id.Should().Be(123);
        document.FileId.Should().Be("doc123");
        document.FileName.Should().Be("document.pdf");
        document.FileSize.Should().Be(2097152);
        document.MimeType.Should().Be("application/pdf");
        document.Url.Should().Be("https://example.com/document.pdf");
    }

    [Fact]
    public void Document_ShouldDeserialize_WithNullableFields()
    {
        // Arrange
        var json = """{"id":123,"fileId":"doc123"}""";

        // Act
        var document = MaxJsonSerializer.Deserialize<Document>(json);

        // Assert
        document.Should().NotBeNull();
        document.Id.Should().Be(123);
        document.FileId.Should().Be("doc123");
        document.FileName.Should().BeNull();
        document.FileSize.Should().BeNull();
        document.MimeType.Should().BeNull();
        document.Url.Should().BeNull();
    }

    [Fact]
    public void Document_ShouldSerialize_ToJson()
    {
        // Arrange
        var document = new Document
        {
            Id = 123,
            FileId = "doc123",
            FileName = "document.pdf",
            FileSize = 2097152,
            MimeType = "application/pdf",
            Url = "https://example.com/document.pdf"
        };

        // Act
        var json = MaxJsonSerializer.Serialize(document);

        // Assert
        json.Should().Contain("\"id\":123");
        json.Should().Contain("\"fileId\":\"doc123\"");
        json.Should().Contain("\"fileName\":\"document.pdf\"");
        json.Should().Contain("\"fileSize\":2097152");
        json.Should().Contain("\"mimeType\":\"application/pdf\"");
        json.Should().Contain("\"url\":\"https://example.com/document.pdf\"");
    }
}

