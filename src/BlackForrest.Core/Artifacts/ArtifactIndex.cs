using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace BlackForrest.Artifacts
{
    public class ArtifactIndex
    {
        public List<ArtifactIndexEntry> Entries { get; set; } = new List<ArtifactIndexEntry>();
        [JsonPropertyName("generated_at")]
        public string GeneratedAt { get; set; } = System.DateTime.UtcNow.ToString("o");
    }
}
