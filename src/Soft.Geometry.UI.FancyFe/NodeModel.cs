using System;
using System.Collections.Generic;
using System.Drawing;

namespace Soft.Geometry.UI.FancyFe
{
    public class NodeModel
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Type { get; set; } = "Extrude";
        public string Title { get; set; } = "Extrude";
        public PointF Position { get; set; } = new PointF(20, 20);
        public SizeF Size { get; set; } = new SizeF(220, 120);
        public Dictionary<string, string> Settings { get; set; } = new Dictionary<string, string>();
        public bool IsShadowMode { get; set; } = false;

        public List<InputSocket> Inputs { get; set; } = new List<InputSocket>();
        public List<OutputSocket> Outputs { get; set; } = new List<OutputSocket>();
    }

    public class InputSocket
    {
        public string Name { get; set; }
        public string Type { get; set; } = "geometry"; // e.g., "Polyline2D"
        public bool IsConnected { get; set; } = false;
        public string ConnectedNodeId { get; set; } = "";
        public string ConnectedOutputName { get; set; } = "";
    }

    public class OutputSocket
    {
        public string Name { get; set; }
        public string Type { get; set; } = "mesh";
        public bool IsConnected { get; set; } = false;
    }
}
