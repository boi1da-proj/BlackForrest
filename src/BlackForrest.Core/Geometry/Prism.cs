using System;
using System.Collections.Generic;
using System.Numerics;

namespace BlackForrest.Geometry
{
    public static class Prism
    {
        // Create a simple prism mesh from a 2D polyline (points in XY plane, Z=0),
        // extruded along Z by "height".
        public static Mesh3D FromPolyline(IList<Vector2> polyline, double height)
        {
            int n = polyline.Count;
            var mesh = new Mesh3D();

            // bottom + top vertices
            for (int i = 0; i < n; i++)
            {
                var p = polyline[i];
                mesh.Vertices.Add(new Vector3(p.X, p.Y, 0));                 // bottom
                mesh.Vertices.Add(new Vector3(p.X, p.Y, (float)height));     // top
            }

            // bottom face (triangle fan)
            for (int i = 1; i < n - 1; i++)
            {
                mesh.Triangles.Add(new int[] { 0, 2 * i, 2 * (i + 1) });
            }

            // top face (reverse orientation)
            for (int i = 1; i < n - 1; i++)
            {
                mesh.Triangles.Add(new int[] { 1, 2 * (i + 1) + 1, 2 * i + 1 });
            }

            // sides
            for (int i = 0; i < n; i++)
            {
                int iNext = (i + 1) % n;
                int bottomA = 2 * i;
                int bottomB = 2 * iNext;
                int topA = 2 * i + 1;
                int topB = 2 * iNext + 1;

                mesh.Triangles.Add(new int[] { bottomA, bottomB, topA });
                mesh.Triangles.Add(new int[] { bottomB, topB, topA });
            }

            return mesh;
        }

        public static Mesh3D FromPolyline3D(IList<Vector3> polyline3D, double height)
        {
            // Optional helper if you want 3D input; extrudes up by height
            var projected = new List<Vector2>();
            foreach (var v in polyline3D)
                projected.Add(new Vector2(v.X, v.Y));
            return FromPolyline(projected, height);
        }
    }

    public class Mesh3D
    {
        public List<Vector3> Vertices { get; } = new List<Vector3>();
        // Each triangle is 3 indices into Vertices
        public List<int[]> Triangles { get; } = new List<int[]>();
    }

    public static class MeshIO
    {
        // Minimal ASCII STL writer
        public static void ExportStl(Mesh3D mesh, string path)
        {
            using var writer = new System.IO.StreamWriter(path);
            writer.WriteLine("solid BlackForrest");
            foreach (var tri in mesh.Triangles)
            {
                var v0 = mesh.Vertices[tri[0]];
                var v1 = mesh.Vertices[tri[1]];
                var v2 = mesh.Vertices[tri[2]];

                var n = ComputeNormal(v0, v1, v2);
                writer.WriteLine($"  facet normal {n.X} {n.Y} {n.Z}");
                writer.WriteLine("    outer loop");
                writer.WriteLine($"      vertex {v0.X} {v0.Y} {v0.Z}");
                writer.WriteLine($"      vertex {v1.X} {v1.Y} {v1.Z}");
                writer.WriteLine($"      vertex {v2.X} {v2.Y} {v2.Z}");
                writer.WriteLine("    endloop");
                writer.WriteLine("  endfacet");
            }
            writer.WriteLine("endsolid BlackForrest");
        }

        private static Vector3 ComputeNormal(Vector3 a, Vector3 b, Vector3 c)
        {
            var ab = b - a;
            var ac = c - a;
            var cross = Vector3.Cross(ab, ac);
            if (cross.Length() == 0) return new Vector3(0, 0, 1);
            return Vector3.Normalize(cross);
        }
    }
}
