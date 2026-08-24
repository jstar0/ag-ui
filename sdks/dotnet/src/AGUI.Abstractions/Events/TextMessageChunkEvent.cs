using System.Text.Json.Serialization;

namespace AGUI.Abstractions;

/// <summary>
/// Compact text message chunk event with optional fields, standing in for a
/// start, content and end sequence for producers that cannot know in advance
/// where a message begins.
/// </summary>
// Keep in sync with sdks/typescript/packages/core/src/events.ts
public sealed class TextMessageChunkEvent : BaseEvent
{
    /// <inheritdoc/>
    [JsonPropertyName("type")]
    public override string Type => AGUIEventTypes.TextMessageChunk;

    /// <summary>
    /// Gets or sets the optional message identifier. Absent continues the
    /// message already open.
    /// </summary>
    [JsonPropertyName("messageId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? MessageId { get; set; }

    /// <summary>
    /// Gets or sets the optional role, on the chunk that opens the message.
    /// </summary>
    [JsonPropertyName("role")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Role { get; set; }

    /// <summary>
    /// Gets or sets the optional content delta.
    /// </summary>
    [JsonPropertyName("delta")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Delta { get; set; }

    /// <summary>
    /// Gets or sets the optional display name for the author.
    /// </summary>
    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Name { get; set; }
}
