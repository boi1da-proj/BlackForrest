namespace Cursor.Tests
{
    public interface IShadowRuntime
    {
        (string status, string summary) Invoke(string module, object inputs);
    }

    public class ShadowRuntimeMock : IShadowRuntime
    {
        public (string status, string summary) Invoke(string module, object inputs)
        {
            // Deterministic response
            return ("success", $"module={module}; inputs_hash=deadbeef");
        }
    }
}
