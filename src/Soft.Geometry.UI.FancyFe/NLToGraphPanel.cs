using System;
using System.Drawing;
using System.Windows.Forms;

namespace Soft.Geometry.UI.FancyFe
{
    public class NLToGraphPanel : Panel
    {
        private readonly TextBox _input;
        private readonly Button _applyButton;
        private readonly GraphModel _graph;

        public NLToGraphPanel(GraphModel graph)
        {
            _graph = graph ?? throw new ArgumentNullException(nameof(graph));
            this.Dock = DockStyle.Left;
            this.Width = 320;
            this.BackColor = BrandTheme.Surface;
            this.BorderStyle = BorderStyle.FixedSingle;

            var header = new Label
            {
                Text = "NL → Graph",
                Dock = DockStyle.Top,
                Height = 26,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                BackColor = BrandTheme.BlueDark,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };
            Controls.Add(header);

            _input = new TextBox
            {
                Multiline = true,
                Dock = DockStyle.Fill,
                ScrollBars = ScrollBars.Vertical,
                Text = "extrude a rectangle height 3"
            };
            Controls.Add(_input);

            _applyButton = new Button
            {
                Text = "Translate",
                Dock = DockStyle.Bottom,
                Height = 34,
                BackColor = BrandTheme.Blue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _applyButton.FlatAppearance.BorderSize = 0;
            _applyButton.Click += OnTranslateClick;
            Controls.Add(_applyButton);
        }

        private void OnTranslateClick(object sender, EventArgs e)
        {
            var updated = NLToGraphTranslator.ApplyCommand(_graph, _input.Text);
            GraphLayoutEngine.ApplyLayout(updated);
            MessageBox.Show("Translation applied and layout updated.", "NL Translator", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
