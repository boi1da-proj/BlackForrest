using System;
using System.Collections.Generic;

namespace Compute.Shadow.Artifacts
{
    public class ArtifactIndexEntry
    {
        public string AssetId { get; set; }            // unique id for the asset
        public string Name { get; set; }               // friendly name
        public string Type { get; set; }               // e.g. "mesh", "definition"
        public string Path { get; set; }               // file path or URL
        public string Version { get; set; }            // semantic version if any
        public string Checksum { get; set; }           // sha256
        public List<string> Dependencies { get; set; } = new();
        public string ShadowDeploymentMetadata { get; set; } // env, runtime, etc.
        public string EnvironmentLabel { get; set; }     // e.g. "dev", "prod"
        public string Timestamp { get; set; }            // ISO 8601

        // Optional helper
        public static string NowIso() => DateTime.UtcNow.ToString("o");
    }
}
