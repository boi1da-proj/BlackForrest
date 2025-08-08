namespace Cursor.Tests
{
    public record CursorPosition(double X, double Y, double Z = 0);

    public record CursorEvent(
        string EventId,
        string SessionId,
        string UserId,
        string EventType,
        CursorPosition Position,
        string? TargetObjectId = null
    );
}
