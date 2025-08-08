using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BlackForrest.UI;

namespace BlackForrest.UI
{
    public class NodeCanvasForm : Form
    {
        private readonly List<Node> _nodes = new();
        private Node _dragging;
        private Point _dragOffset;

        public NodeCanvasForm()
        {
            // White UI base with strong brand accents
            this.Text = "BlackForrest — Node Canvas (Rutten-inspired)";
            this.Width = 980;
            this.Height = 640;
            this.BackColor = BrandTheme.Background;
            this.ForeColor = BrandTheme.Text;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.DoubleBuffered = true;

            // Optional header bar mimic (brand accent)
            var header = new Panel
            {
                Size = new Size(this.ClientSize.Width, 60),
                Location = new Point(0, 0),
                BackColor = BrandTheme.Surface
            };
            header.Paint += (s, e) =>
            {
                // subtle bottom border line
                using (var pen = new Pen(BrandTheme.Border, 1f))
                {
                    e.Graphics.DrawLine(pen, 0, header.Height - 1, header.Width, header.Height - 1);
                }
            };
            this.Controls.Add(header);

            // Seed a minimal node ( Extrude )
            AddNode("Extrude", new Point(20, 90), new[] { "Polyline2D", "Height" }, new[] { "Mesh3D" });

            // Quick reset / Run button
            var runBtn = new Button
            {
                Text = "Run",
                Location = new Point(12, 12),
                Width = 60,
                Height = 28,
                FlatStyle = FlatStyle.Flat,
                BackColor = BrandTheme.Blue,
                ForeColor = Color.White
            };
            runBtn.FlatAppearance.BorderSize = 0;
            this.Controls.Add(runBtn);
            runBtn.Click += (s, e) => MessageBox.Show("Execution pipeline would run here (stub).");

            // Allow dragging nodes
            this.MouseDown += (s, e) =>
            {
                var hit = HitTestNode(e.Location);
                _dragging = hit;
                if (_dragging != null)
                    _dragOffset = new Point(e.X - _dragging.Panel.Left, e.Y - _dragging.Panel.Top);
            };
            this.MouseMove += (s, e) =>
            {
                if (_dragging != null)
                {
                    _dragging.Panel.Left = e.X - _dragOffset.X;
                    _dragging.Panel.Top  = e.Y - _dragOffset.Y;
                }
                this.Invalidate();
            };
            this.MouseUp += (s, e) => _dragging = null;
        }

        private void AddNode(string title, Point location, string[] inputs, string[] outputs)
        {
            var node = new Node(title, location, inputs, outputs);
            _nodes.Add(node);
            this.Controls.Add(node.Panel);
        }

        private Node HitTestNode(Point p)
        {
            foreach (var n in _nodes)
            {
                var r = new Rectangle(n.Panel.Left, n.Panel.Top, n.Panel.Width, n.Panel.Height);
                if (r.Contains(p))
                    return n;
            }
            return null;
        }

        // Lightweight Node container
        public class Node
        {
            public Panel Panel { get; }

            public Node(string title, Point location, string[] inputs, string[] outputs)
            {
                Panel = new Panel
                {
                    Location = location,
                    Size = new Size(260, Math.Max(120, 40 + inputs.Length * 18)),
                    BackColor = BrandTheme.Surface,
                    BorderStyle = BorderStyle.FixedSingle
                };

                // Title bar
                var titleBar = new Panel
                {
                    Height = 28,
                    Dock = DockStyle.Top,
                    BackColor = BrandTheme.Primary
                };
                var titleLabel = new Label
                {
                    Text = title,
                    ForeColor = Color.White,
                    Font = new System.Drawing.Font("Segoe UI", 9.5f, System.Drawing.FontStyle.Bold),
                    Dock = DockStyle.Fill,
                    TextAlign = System.Drawing.ContentAlignment.MiddleCenter
                };
                titleBar.Controls.Add(titleLabel);
                Panel.Controls.Add(titleBar);

                // Inputs
                var inLabel = new Label { Text = "Inputs:", ForeColor = BrandTheme.Text, Dock = DockStyle.Top, Height = 14 };
                Panel.Controls.Add(inLabel);
                var inPanel = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true };
                foreach (var inp in inputs)
                    inPanel.Controls.Add(new Label { Text = inp, ForeColor = Color.FromArgb(60,60,60), AutoSize = true });
                Panel.Controls.Add(inPanel);

                // Outputs
                var outLabel = new Label { Text = "Outputs:", ForeColor = BrandTheme.Text, Dock = DockStyle.Top, Height = 14 };
                Panel.Controls.Add(outLabel);
                var outPanel = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true };
                foreach (var o in outputs)
                    outPanel.Controls.Add(new Label { Text = o, ForeColor = Color.FromArgb(60,60,60), AutoSize = true });
                Panel.Controls.Add(outPanel);
            }
        }
    }
}
