using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Compute.Shadow.Geometry;

namespace Compute.Shadow.Bridge
{
    // Local, in-process compute path. This guarantees a working baseline.
    public class LocalComputeClient : IComputeClient
    {
        public Task<Mesh3D> ExtrudePolylineAsync(IList<Vector2> polyline, double height)
        {
            // Simple, synchronous path wrapped in Task
            Mesh3D mesh = Compute.Shadow.Geometry.Prism.FromPolyline(polyline, height);
            return Task.FromResult(mesh);
        }
    }
}
