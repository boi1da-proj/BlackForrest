using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using BlackForrest.Core;
using BlackForrest.Geometry;
using BlackForrest.ComputeBridge;
using BlackForrest.Artifacts;
using BlackForrest.UI;

namespace BlackForrest.App
{
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            Console.WriteLine("BlackForrest v1.0 - 2D/3D engine with local compute and optional remote compute.");

            // Simple demonstration: create a 2D polyline and extrude to 3D
            var polyline = new List<Vector2> { new Vector2(0,0), new Vector2(6,0), new Vector2(6,4), new Vector2(0,4) };
            double height = 3.0;

            // Create a local compute client (default)
            IComputeClient compute = new LocalComputeClient();

            // Extrude via compute (local)
            Mesh3D mesh = await compute.ExtrudePolylineAsync(polyline, height);

            // Persist to STL for quick check
            string outDir = Path.Combine(Directory.GetCurrentDirectory(), "outputs");
            Directory.CreateDirectory(outDir);
            string stlPath = Path.Combine(outDir, "prism_local.stl");
            MeshIO.ExportStl(mesh, stlPath);
            Console.WriteLine($"Exported local prism to {stlPath}");

            // Optional: remote compute
            // IComputeClient remote = new RhinoComputeClient("http://localhost:8000");
            // var remoteMesh = await remote.ExtrudePolylineAsync(polyline, height);

            // Launch a simple Rutten-style UI
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var form = new BlackForrest.UI.NodeCanvasForm();
            Application.Run(form);

            // Artifact index example (static demo)
            var index = new ArtifactIndex();
            string artifactPath = Path.GetFullPath("./artifact_index.json");
            var entry = new ArtifactIndexEntry
            {
                AssetId = "prism_local_001",
                Name = "Prism Local (2D polyline extrusion)",
                Type = "mesh",
                Path = stlPath,
                Version = "1.0.0",
                Checksum = "TODO_SHA256",
                Dependencies = new List<string> { "BlackForrest.Core", "BlackForrest.UI" },
                ShadowDeploymentMetadata = "local",
                EnvironmentLabel = "dev",
                Timestamp = DateTime.UtcNow.ToString("o")
            };
            index.Entries.Add(entry);
            string json = System.Text.Json.JsonSerializer.Serialize(index, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(artifactPath, json);
            Console.WriteLine($"Wrote artifact_index.json to {artifactPath}");
        }
    }
}
