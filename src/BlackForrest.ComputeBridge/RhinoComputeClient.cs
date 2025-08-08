using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BlackForrest.Geometry;

namespace BlackForrest.ComputeBridge
{
    // Template for remote Rhino.Compute integration.
    // Replace endpoint/payload shape with the actual Compute REST contract you adopt.
    public class RhinoComputeClient : IComputeClient
    {
        private readonly string _serverUrl;
        private readonly HttpClient _http;

        public RhinoComputeClient(string serverUrl)
        {
            _serverUrl = serverUrl?.TrimEnd('/');
            _http = new HttpClient();
        }

        public async Task<Mesh3D> ExtrudePolylineAsync(System.Collections.Generic.IList<Vector2> polyline, double height)
        {
            // TODO: Build proper GH Definition + inputs, then POST to Rhino.Compute
            // The following is a skeleton to show you where to hook in the real payload.

            var requestPayload = new
            {
                operation = "extrude",
                inputs = new
                {
                    polyline = polyline,
                    height = height
                }
            };

            var json = JsonSerializer.Serialize(requestPayload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Example endpoint (adjust to your actual compute API)
            var resp = await _http.PostAsync($"{_serverUrl}/compute/extrudePolyline", content);
            resp.EnsureSuccessStatusCode();

            // Expect a payload describing a mesh (vertices + triangles)
            var respJson = await resp.Content.ReadAsStringAsync();
            var meshDto = JsonSerializer.Deserialize<MeshDto>(respJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Translate DTO to internal Mesh3D
            var mesh = new Mesh3D();
            foreach (var v in meshDto.Vertices)
                mesh.Vertices.Add(new System.Numerics.Vector3((float)v.X, (float)v.Y, (float)v.Z));
            mesh.Triangles.AddRange(meshDto.Triangles);

            return mesh;
        }

        // DTO to deserialize remote mesh
        private class MeshDto
        {
            public List<Vector3Dto> Vertices { get; set; }
            public List<int[]> Triangles { get; set; }
        }

        private class Vector3Dto
        {
            public float X { get; set; }
            public float Y { get; set; }
            public float Z { get; set; }
        }
    }
}
