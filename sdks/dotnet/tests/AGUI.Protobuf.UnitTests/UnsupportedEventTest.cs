using System;
using AGUI.Abstractions;
using Xunit;
using Proto = AGUI.ProtocolBuffers;

namespace AGUI.Protobuf.UnitTests;

// Every schema event with a .NET model crosses the binary transport since the
// wire mappers are generated from the schema. The remaining exceptions are
// the three subagent events, whose .NET models land with #2350: the wire
// entries are recognised and explicitly unsupported — never surfaced as some
// other event.
public sealed class UnsupportedEventTest
{
    public static TheoryData<BaseEvent> NewlyRepresentableEvents() => new()
    {
        new ReasoningStartEvent { MessageId = "m1" },
        new ReasoningEndEvent { MessageId = "m1" },
        new ReasoningMessageStartEvent { MessageId = "m1" },
        new ReasoningMessageContentEvent { MessageId = "m1", Delta = "d" },
        new ReasoningMessageEndEvent { MessageId = "m1" },
        new ReasoningMessageChunkEvent { MessageId = "m1", Delta = "d" },
        new ReasoningEncryptedValueEvent { Subtype = "message", EntityId = "m1", EncryptedValue = "v" },
        new ActivitySnapshotEvent { MessageId = "m1", ActivityType = "a", Content = JsonTestHelpers.Parse("{}") },
        new ActivityDeltaEvent { MessageId = "m1", ActivityType = "a", Patch = JsonTestHelpers.Parse("[]") },
        new ToolCallResultEvent { MessageId = "m1", ToolCallId = "c1", Content = "ok" },
        new TextMessageChunkEvent { MessageId = "m1", Delta = "d" },
        new ToolCallChunkEvent { ToolCallId = "c1", Delta = "{" },
    };

    [Theory]
    [MemberData(nameof(NewlyRepresentableEvents))]
    public void FormerlyUnsupportedEvent_RoundTrips(BaseEvent evt)
    {
        var decoded = AGUIProtobuf.Decode(AGUIProtobuf.Encode(evt));
        Assert.Equal(evt.Type, decoded.Type);
    }

    // Proto.Event is internal, so the theory parameterises by name and the
    // envelope is built inside the test body.
    private static Proto.Event SubagentEnvelope(string kind) => kind switch
    {
        "SUBAGENT_STARTED" => new Proto.Event
        {
            SubagentStarted = new Proto.SubagentStartedEvent
            {
                BaseEvent = new Proto.BaseEvent { Type = Proto.EventType.SubagentStarted },
                SubagentRunId = "s1",
                Name = "researcher",
            },
        },
        "SUBAGENT_FINISHED" => new Proto.Event
        {
            SubagentFinished = new Proto.SubagentFinishedEvent
            {
                BaseEvent = new Proto.BaseEvent { Type = Proto.EventType.SubagentFinished },
                SubagentRunId = "s1",
            },
        },
        _ => new Proto.Event
        {
            SubagentError = new Proto.SubagentErrorEvent
            {
                BaseEvent = new Proto.BaseEvent { Type = Proto.EventType.SubagentError },
                SubagentRunId = "s1",
                Message = "boom",
            },
        },
    };

    [Theory]
    [InlineData("SUBAGENT_STARTED")]
    [InlineData("SUBAGENT_FINISHED")]
    [InlineData("SUBAGENT_ERROR")]
    public void Decode_SubagentEvent_ThrowsExplicitlyUntil2350(string kind)
    {
        var bytes = Google.Protobuf.MessageExtensions.ToByteArray(SubagentEnvelope(kind));
        var exception = Assert.Throws<NotSupportedException>(() => AGUIProtobuf.Decode(bytes));
        Assert.Contains("#2350", exception.Message);
    }

    [Fact]
    public void Encode_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => AGUIProtobuf.Encode(null!));
    }
}
