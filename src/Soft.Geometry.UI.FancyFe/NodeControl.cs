using System.Drawing;
using System.Windows.Forms;

namespace Soft.Geometry.UI.FancyFe
{
    public class NodeControl : UserControl
    {
        public NodeModel Model { get; private set; }
        public bool IsSelected { get; set; }
        public bool IsShadowMode { get; set; }

        public NodeControl(NodeModel model)
        {
            Model = model;
            this.Width = (int)model.Size.Width;
            this.Height = (int)model.Size.Height;
            this.Left = (int)model.Position.X;
            this.Top = (int)model.Position.Y;
            this.DoubleBuffered = true;

            BuildUi();
        }

        private void BuildUi()
        {
            Controls.Clear();
            this.BackColor = BrandTheme.Surface;
            this.BorderStyle = BorderStyle.None; // custom border in OnPaint

            var titleBar = new Panel
            {
                Height = 28,
                Dock = DockStyle.Top,
                BackColor = GetTitleBarColor()
            };
            var titleLbl = new Label
            {
                Text = Model.Title,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            titleBar.Controls.Add(titleLbl);
            Controls.Add(titleBar);

            var body = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = BrandTheme.Surface
            };
            Controls.Add(body);

            // Inputs indicator
            if (Model.Inputs != null && Model.Inputs.Count > 0)
            {
                var inDot = new Panel
                {
                    Size = new Size(10, 10),
                    Location = new Point(8, titleBar.Bottom + 6),
                    BackColor = GetSocketColor(Model.Inputs[0].Type, dark: false)
                };
                body.Controls.Add(inDot);
            }
            // Outputs indicator
            if (Model.Outputs != null && Model.Outputs.Count > 0)
            {
                var outDot = new Panel
                {
                    Size = new Size(10, 10),
                    Location = new Point(this.Width - 20, titleBar.Bottom + 6),
                    BackColor = GetSocketColor(Model.Outputs[0].Type, dark: true)
                };
                body.Controls.Add(outDot);
            }
        }

        private Color GetTitleBarColor()
        {
            if (IsShadowMode) return BrandTheme.ShadowMode;
            return Model.Type switch
            {
                "Extrude" => BrandTheme.Primary,
                "Loft" => BrandTheme.NeonCyan,
                "Boolean" => BrandTheme.Purple,
                "Transform" => BrandTheme.Yellow,
                "Fillet" => BrandTheme.Green,
                _ => BrandTheme.Primary
            };
        }

        private static Color GetSocketColor(string dataType, bool dark)
        {
            // Map common types to brand accents
            Color baseColor = dataType switch
            {
                var t when string.IsNullOrWhiteSpace(t) => BrandTheme.Primary,
                var t when t.ToLower().Contains("polyline") => BrandTheme.NeonCyan,
                var t when t.ToLower().Contains("mesh") => BrandTheme.Purple,
                var t when t.ToLower().Contains("surface") => BrandTheme.Blue,
                var t when t.ToLower().Contains("number") => BrandTheme.Yellow,
                _ => BrandTheme.Green
            };
            return dark ? BrandTheme.Darken(baseColor, 0.25) : baseColor;
        }

        public void UpdatePosition(PointF pos)
        {
            Model.Position = pos;
            this.Left = (int)pos.X;
            this.Top = (int)pos.Y;
            Invalidate();
        }

        public void UpdateSelection(bool selected)
        {
            IsSelected = selected;
            Invalidate();
        }

        public void UpdateShadowMode(bool shadowMode)
        {
            IsShadowMode = shadowMode;
            Model.IsShadowMode = shadowMode;
            BuildUi();
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            // Darker border for contrast
            var borderColor = BrandTheme.Darken(BrandTheme.Border, 0.25);
            using var pen = new Pen(borderColor, 1.5f);
            e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);

            if (IsSelected)
            {
                using var selPen = new Pen(BrandTheme.PrimaryDark, 2f);
                e.Graphics.DrawRectangle(selPen, 2, 2, Width - 5, Height - 5);
            }
        }
    }
}
