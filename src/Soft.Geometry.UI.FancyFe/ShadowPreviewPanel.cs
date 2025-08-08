using System.Drawing;
using System.Windows.Forms;

namespace Soft.Geometry.UI.FancyFe
{
    // Lightweight in-UI sandbox badge to indicate Shadow Code mode
    public class ShadowPreviewPanel : Panel
    {
        private readonly Label _badge;
        public bool IsShadowActive { get; private set; }

        public ShadowPreviewPanel()
        {
            this.Height = 28;
            this.Dock = DockStyle.Top;
            this.BackColor = Color.White;
            this.BorderStyle = BorderStyle.FixedSingle;

            _badge = new Label
            {
                Text = "Shadow OFF",
                ForeColor = Color.White,
                BackColor = Color.Gray,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };
            this.Controls.Add(_badge);
        }

        public void SetShadowActive(bool isActive)
        {
            IsShadowActive = isActive;
            _badge.Text = IsShadowActive ? "Shadow ON" : "Shadow OFF";
            _badge.BackColor = IsShadowActive ? BrandTheme.Green : Color.Gray;
        }
    }
}
