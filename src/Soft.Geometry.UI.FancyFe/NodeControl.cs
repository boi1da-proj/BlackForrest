using System.Drawing;
using System.Windows.Forms;

namespace Soft.Geometry.UI.FancyFe
{
    public class NodeControl : UserControl
    {
        private const int ResizeGripSize = 12;

        public NodeModel Model { get; private set; }
        public bool IsSelected { get; set; } = false;
        public bool IsShadowMode { get; set; } = false;

        private bool _isResizing;
        private Point _resizeStartMouse;
        private Size _resizeStartSize;

        public NodeControl(NodeModel model)
        {
            Model = model;
            this.Width = (int)model.Size.Width;
            this.Height = (int)model.Size.Height;
            this.Left = (int)model.Position.X;
            this.Top = (int)model.Position.Y;
            this.DoubleBuffered = true;

            SetupVisuals();

            this.MouseDown += OnMouseDownInternal;
            this.MouseMove += OnMouseMoveInternal;
            this.MouseUp += OnMouseUpInternal;
            this.Cursor = Cursors.Default;
        }

        private void SetupVisuals()
        {
            // Clear any previous children to avoid duplicates on rebuild
            this.Controls.Clear();

            // Title bar with neon styling
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

            // Shadow mode indicator
            if (IsShadowMode)
            {
                var shadowBadge = new Label
                {
                    Text = "🔒",
                    ForeColor = BrandTheme.SandboxIndicator,
                    Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                    AutoSize = true,
                    Location = new Point(8, 4)
                };
                titleBar.Controls.Add(shadowBadge);
            }

            titleBar.Controls.Add(titleLbl);
            this.Controls.Add(titleBar);

            // Body area with subtle border
            var body = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = BrandTheme.Surface,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Input sockets
            var inputsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 20 + (Model.Inputs.Count * 16),
                Padding = new Padding(8, 4, 8, 4)
            };

            foreach (var input in Model.Inputs)
            {
                var socketLbl = new Label
                {
                    Text = $"● {input.Name}",
                    ForeColor = input.IsConnected ? BrandTheme.Green : BrandTheme.NeonCyan,
                    Font = new Font("Segoe UI", 8f, FontStyle.Regular),
                    AutoSize = true,
                    Margin = new Padding(0, 2, 0, 2)
                };
                inputsPanel.Controls.Add(socketLbl);
            }

            body.Controls.Add(inputsPanel);

            // Output sockets
            var outputsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 20 + (Model.Outputs.Count * 16),
                Padding = new Padding(8, 4, 8, 4)
            };

            foreach (var output in Model.Outputs)
            {
                var socketLbl = new Label
                {
                    Text = $"{output.Name} ●",
                    ForeColor = output.IsConnected ? BrandTheme.Green : BrandTheme.Purple,
                    Font = new Font("Segoe UI", 8f, FontStyle.Regular),
                    AutoSize = true,
                    Margin = new Padding(0, 2, 0, 2)
                };
                outputsPanel.Controls.Add(socketLbl);
            }

            body.Controls.Add(outputsPanel);

            // Settings display
            if (Model.Settings.Count > 0)
            {
                var settingsPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    Padding = new Padding(8, 4, 8, 4)
                };

                var settingsLbl = new Label
                {
                    Text = "Settings: " + string.Join(", ", Model.Settings.Keys),
                    ForeColor = BrandTheme.Text,
                    Font = new Font("Segoe UI", 8f, FontStyle.Italic),
                    AutoSize = true,
                    Location = new Point(0, 0)
                };
                settingsPanel.Controls.Add(settingsLbl);
                body.Controls.Add(settingsPanel);
            }

            this.Controls.Add(body);
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

        public void UpdatePosition(PointF pos)
        {
            Model.Position = pos;
            this.Left = (int)pos.X;
            this.Top = (int)pos.Y;
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
            SetupVisuals(); // Recreate visuals with new shadow mode
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Draw selection outline
            if (IsSelected)
            {
                using var selPen = new Pen(Color.FromArgb(255, 0, 128), 2f);
                e.Graphics.DrawRectangle(selPen, 1, 1, this.Width - 3, this.Height - 3);
            }

            // Draw resize grip
            using var gripBrush = new SolidBrush(Color.FromArgb(160, 160, 160));
            var gripRect = new Rectangle(this.Width - ResizeGripSize, this.Height - ResizeGripSize, ResizeGripSize, ResizeGripSize);
            e.Graphics.FillRectangle(gripBrush, gripRect);
        }

        private void OnMouseDownInternal(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && IsInResizeGrip(e.Location))
            {
                _isResizing = true;
                _resizeStartMouse = PointToScreen(e.Location);
                _resizeStartSize = this.Size;
                this.Capture = true;
            }
        }

        private void OnMouseMoveInternal(object sender, MouseEventArgs e)
        {
            if (_isResizing)
            {
                var current = PointToScreen(e.Location);
                int dx = current.X - _resizeStartMouse.X;
                int dy = current.Y - _resizeStartMouse.Y;
                int newW = System.Math.Max(160, _resizeStartSize.Width + dx);
                int newH = System.Math.Max(100, _resizeStartSize.Height + dy);
                this.Size = new Size(newW, newH);
                Model.Size = new SizeF(newW, newH);
                Invalidate();
            }
            else
            {
                this.Cursor = IsInResizeGrip(e.Location) ? Cursors.SizeNWSE : Cursors.Default;
            }
        }

        private void OnMouseUpInternal(object sender, MouseEventArgs e)
        {
            if (_isResizing)
            {
                _isResizing = false;
                this.Capture = false;
            }
        }

        private bool IsInResizeGrip(Point p)
        {
            return p.X >= this.Width - ResizeGripSize && p.Y >= this.Height - ResizeGripSize;
        }
    }
}
