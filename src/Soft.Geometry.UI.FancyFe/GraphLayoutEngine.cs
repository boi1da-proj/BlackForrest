using System.Drawing;

namespace Soft.Geometry.UI.FancyFe
{
    public static class GraphLayoutEngine
    {
        public static void ApplyLayout(GraphModel graph)
        {
            if (graph == null || graph.Nodes == null || graph.Nodes.Count == 0) return;

            float x = 60f;
            float y = 60f;
            float stepX = 280f;
            float stepY = 160f;
            int perRow = 4;

            for (int i = 0; i < graph.Nodes.Count; i++)
            {
                var node = graph.Nodes[i];
                int row = i / perRow;
                int col = i % perRow;
                node.Position = new PointF(x + col * stepX, y + row * stepY);
            }
        }
    }
}
