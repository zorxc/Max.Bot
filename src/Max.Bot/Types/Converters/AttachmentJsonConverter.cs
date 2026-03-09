using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Max.Bot.Types;

namespace Max.Bot.Types.Converters;

/// <summary>
/// JSON converter for polymorphic deserialization of Attachment objects based on the "type" field.
/// </summary>
public class AttachmentJsonConverter : JsonConverter<Attachment>
{
    /// <inheritdoc />
    public override Attachment? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected StartObject token.");
        }

        // Parse the JSON document to read properties
        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;

        // Determine the attachment type based on properties
        string? typeString = null;
        if (root.TryGetProperty("type", out var typeElement) && typeElement.ValueKind == JsonValueKind.String)
        {
            typeString = typeElement.GetString();
        }

        if (IsType(typeString, AttachmentTypeNames.InlineKeyboard))
        {
            return JsonSerializer.Deserialize<InlineKeyboardAttachment>(root.GetRawText(), options);
        }

        if (root.TryGetProperty("photo", out _))
        {
            return JsonSerializer.Deserialize<PhotoAttachment>(root.GetRawText(), options);
        }

        if (root.TryGetProperty("video", out _))
        {
            return JsonSerializer.Deserialize<VideoAttachment>(root.GetRawText(), options);
        }

        if (root.TryGetProperty("audio", out _))
        {
            return JsonSerializer.Deserialize<AudioAttachment>(root.GetRawText(), options);
        }

        if (root.TryGetProperty("document", out _))
        {
            return JsonSerializer.Deserialize<DocumentAttachment>(root.GetRawText(), options);
        }

        if (IsType(typeString, AttachmentTypeNames.Image))
        {
            return JsonSerializer.Deserialize<ImageAttachment>(root.GetRawText(), options);
        }

        if (IsType(typeString, AttachmentTypeNames.File))
        {
            return JsonSerializer.Deserialize<DocumentAttachment>(root.GetRawText(), options);
        }

        if (IsType(typeString, AttachmentTypeNames.Location))
        {
            return JsonSerializer.Deserialize<LocationAttachment>(root.GetRawText(), options);
        }

        if (IsType(typeString, AttachmentTypeNames.Contact))
        {
            return JsonSerializer.Deserialize<ContactAttachment>(root.GetRawText(), options);
        }

        // Default to document for unknown types
        return JsonSerializer.Deserialize<DocumentAttachment>(root.GetRawText(), options);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, Attachment value, JsonSerializerOptions options)
    {
        // Serialize based on the actual type
        switch (value)
        {
            case PhotoAttachment photoAttachment:
                JsonSerializer.Serialize(writer, photoAttachment, options);
                break;
            case VideoAttachment videoAttachment:
                JsonSerializer.Serialize(writer, videoAttachment, options);
                break;
            case AudioAttachment audioAttachment:
                JsonSerializer.Serialize(writer, audioAttachment, options);
                break;
            case DocumentAttachment documentAttachment:
                JsonSerializer.Serialize(writer, documentAttachment, options);
                break;
            case LocationAttachment locationAttachment:
                JsonSerializer.Serialize(writer, locationAttachment, options);
                break;
            case ContactAttachment contactAttachment:
                JsonSerializer.Serialize(writer, contactAttachment, options);
                break;
            case ImageAttachment imageAttachment:
                JsonSerializer.Serialize(writer, imageAttachment, options);
                break;
            case InlineKeyboardAttachment inlineKeyboardAttachment:
                JsonSerializer.Serialize(writer, inlineKeyboardAttachment, options);
                break;
            default:
                throw new JsonException($"Unknown attachment type: {value.GetType()}");
        }
    }

    private static bool IsType(string? actualType, string expectedType)
    {
        return !string.IsNullOrWhiteSpace(actualType) &&
               actualType.Equals(expectedType, StringComparison.OrdinalIgnoreCase);
    }
}

