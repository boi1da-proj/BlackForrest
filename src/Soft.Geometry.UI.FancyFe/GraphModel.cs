using System.Collections.Generic;

namespace Soft.Geometry.UI.FancyFe
{
    public class GraphModel
    {
        public string Name { get; set; } = "Untitled Graph";
        public string Version { get; set; } = "0.1";
        public List<NodeModel> Nodes { get; set; } = new List<NodeModel>();
        public List<ConnectionModel> Connections { get; set; } = new List<ConnectionModel>();
        public bool IsShadowMode { get; set; } = false;
    }
}
