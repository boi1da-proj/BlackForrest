using System;
using System.Drawing;
using System.Windows.Forms;

namespace Soft.Geometry.UI.FancyFe
{
    public class Viewport3DPanel : Panel
    {
        public Viewport3DPanel()
        {
            this.Dock = DockStyle.Right;
            this.Width = 340;
            this.BackColor = Color.White;
            this.BorderStyle = BorderStyle.FixedSingle;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            float w = this.ClientSize.Width;
            float h = this.ClientSize.Height;
            float s = Math.Min(w, h) * 0.35f;
            float cx = w * 0.5f;
            float cy = h * 0.55f;

            // Simple isometric-ish cube
            PointF p1 = new PointF(cx - s, cy - s);
            PointF p2 = new PointF(cx + s, cy - s);
            PointF p3 = new PointF(cx + s, cy + s);
            PointF p4 = new PointF(cx - s, cy + s);

            float dx = s * 0.6f;
            float dy = s * 0.4f;
            PointF q1 = new PointF(p1.X + dx, p1.Y - dy);
            PointF q2 = new PointF(p2.X + dx, p2.Y - dy);
            PointF q3 = new PointF(p3.X + dx, p3.Y - dy);
            PointF q4 = new PointF(p4.X + dx, p4.Y - dy);

            using var pen = new Pen(BrandTheme.BlueDark, 2f);
            // Front square
            g.DrawRectangle(pen, p1.X, p1.Y, 2 * s, 2 * s);
            // Back square
            g.DrawRectangle(pen, q1.X, q1.Y, 2 * s, 2 * s);
            // Connectors
            g.DrawLine(pen, p1, q1);
            g.DrawLine(pen, p2, q2);
            g.DrawLine(pen, p3, q3);
            g.DrawLine(pen, p4, q4);
        }
    }
}
