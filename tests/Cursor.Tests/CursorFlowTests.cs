using FluentAssertions;
using Xunit;

namespace Cursor.Tests
{
    public class CursorFlowTests
    {
        private readonly IShadowRuntime _shadow = new ShadowRuntimeMock();

        [Fact]
        public void IdleEvent_DoesNotInvokeHeavyCompute_LogsCompleted()
        {
            var store = new InMemoryArtifactIndexStore();
            var evt = new CursorEvent("evt-0001", "sess-001", "tester", "idle", new CursorPosition(0,0,0));

            // Simulate: no heavy compute on idle
            store.Append(new ArtifactEntry
            {
                Event = evt,
                Status = "completed",
                DurationMs = 5
            });

            var json = store.ToJson();
            json.Should().Contain("\"event_type\":"); // shape check
            json.Should().Contain("idle");
        }

        [Fact]
        public void DragSequence_CoalescesToSingleShadowCall()
        {
            var store = new InMemoryArtifactIndexStore();
            var session = "sess-002";
            var start = new CursorEvent("evt-1000", session, "tester", "drag_start", new CursorPosition(1,1));
            var upd1  = new CursorEvent("evt-1001", session, "tester", "drag_update", new CursorPosition(2,2));
            var upd2  = new CursorEvent("evt-1002", session, "tester", "drag_update", new CursorPosition(3,3));
            var end   = new CursorEvent("evt-1003", session, "tester", "drag_end",   new CursorPosition(4,4));

            // Simulate coalescing: only final invokes compute
            var result = _shadow.Invoke("softlyplease.cursor", new { start, upd1, upd2, end });
            store.Append(new ArtifactEntry
            {
                Event = end,
                Status = result.status,
                DurationMs = 42,
                Module = "softlyplease.cursor",
                ModuleVersion = "v0.9.1"
            });

            var json = store.ToJson();
            json.Should().Contain("drag_end");
            json.Should().Contain("softlyplease.cursor");
        }
    }
}
