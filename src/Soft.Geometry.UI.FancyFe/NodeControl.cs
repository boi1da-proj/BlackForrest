using System.Drawing;
using System.Windows.Forms;

namespace Soft.Geometry.UI.FancyFe
{
    public class NodeControl : UserControl
    {
        public NodeModel Model { get; private set; }
        public bool IsSelected { get; set; } = false;
        public bool IsShadowMode { get; set; } = false;

        public NodeControl(NodeModel model)
        {
            Model = model;
            this.Width = (int)model.Size.Width;
            this.Height = (int)model.Size.Height;
            this.Left = (int)model.Position.X;
            this.Top = (int)model.Position.Y;
            this.DoubleBuffered = true;

            SetupVisuals();
        }

        private void SetupVisuals()
        {
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
            if (selected)
            {
                this.BorderStyle = BorderStyle.Fixed3D;
                this.BackColor = BrandTheme.SelectionHighlight;
            }
            else
            {
                this.BorderStyle = BorderStyle.None;
                this.BackColor = BrandTheme.Surface;
            }
        }

        public void UpdateShadowMode(bool shadowMode)
        {
            IsShadowMode = shadowMode;
            Model.IsShadowMode = shadowMode;
            SetupVisuals(); // Recreate visuals with new shadow mode
        }
    }
}
