using System.Text.Json;
using System.Text.Json.Serialization;
using Max.Bot.Types;
using Max.Bot.Types.Enums;

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
        MessageType? attachmentType = null;
        if (root.TryGetProperty("type", out var typeElement))
        {
            var typeString = typeElement.GetString();
            if (!string.IsNullOrEmpty(typeString))
            {
                // Convert snake_case to PascalCase for enum parsing
                var enumValue = typeString.Replace("_", string.Empty);
                if (Enum.TryParse<MessageType>(enumValue, true, out var parsedType))
                {
                    attachmentType = parsedType;
                }
            }
        }

        // Deserialize based on type and available properties
        Attachment? attachment = null;
        if (attachmentType == MessageType.Image || root.TryGetProperty("photo", out _))
        {
            attachment = JsonSerializer.Deserialize<PhotoAttachment>(root.GetRawText(), options);
        }
        else if (attachmentType == MessageType.File)
        {
            if (root.TryGetProperty("video", out _))
            {
                attachment = JsonSerializer.Deserialize<VideoAttachment>(root.GetRawText(), options);
            }
            else if (root.TryGetProperty("audio", out _))
            {
                attachment = JsonSerializer.Deserialize<AudioAttachment>(root.GetRawText(), options);
            }
            else if (root.TryGetProperty("document", out _))
            {
                attachment = JsonSerializer.Deserialize<DocumentAttachment>(root.GetRawText(), options);
            }
            else
            {
                // Default to document for unknown file types
                attachment = JsonSerializer.Deserialize<DocumentAttachment>(root.GetRawText(), options);
            }
        }
        else
        {
            // Default to document for unknown types
            attachment = JsonSerializer.Deserialize<DocumentAttachment>(root.GetRawText(), options);
        }

        return attachment;
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
            default:
                throw new JsonException($"Unknown attachment type: {value.GetType()}");
        }
    }

}

