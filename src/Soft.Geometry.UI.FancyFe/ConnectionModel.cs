namespace Soft.Geometry.UI.FancyFe
{
    public class ConnectionModel
    {
        public string Id { get; set; } = System.Guid.NewGuid().ToString("N");
        public string SourceNodeId { get; set; }
        public string SourceOutput { get; set; }
        public string TargetNodeId { get; set; }
        public string TargetInput { get; set; }
        public bool IsShadowMode { get; set; } = false;
    }
}
