using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Compute.Shadow.Geometry;

namespace Compute.Shadow.Bridge
{
    public interface IComputeClient
    {
        // Offload a 2D polyline extrusion
        Task<Mesh3D> ExtrudePolylineAsync(IList<Vector2> polyline, double height);

        // A place to plug in more operations later (Boolean, Loft, etc.)
    }
}
