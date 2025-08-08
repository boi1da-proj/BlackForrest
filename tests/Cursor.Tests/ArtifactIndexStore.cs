using System.Text.Json;

namespace Cursor.Tests
{
    public class ArtifactEntry
    {
        public string SchemaVersion { get; set; } = "1.0";
        public string EnvironmentLabel { get; set; } = "ci-local";
        public string Module { get; set; } = "softlyplease.cursor";
        public string ModuleVersion { get; set; } = "v0.9.1";
        public string Status { get; set; } = "completed";
        public double DurationMs { get; set; }
        public object Event { get; set; } = new();
    }

    public interface IArtifactIndexStore
    {
        void Append(ArtifactEntry entry);
        string ToJson();
        void FlushTo(string filePath);
    }

    public class InMemoryArtifactIndexStore : IArtifactIndexStore
    {
        private readonly List<ArtifactEntry> _entries = new();
        public void Append(ArtifactEntry entry) => _entries.Add(entry);
        public string ToJson() => JsonSerializer.Serialize(new { entries = _entries }, new JsonSerializerOptions { WriteIndented = true });
        public void FlushTo(string filePath) => File.WriteAllText(filePath, ToJson());
    }
}
