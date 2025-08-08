using System;
using System.Drawing;
using System.Windows.Forms;

namespace Soft.Geometry.UI.FancyFe
{
    public class NodeInspectorPanel : UserControl
    {
        private NodeModel _currentNode;
        private TableLayoutPanel _propertiesPanel;
        private Label _titleLabel;

        public event Action<NodeModel> NodePropertyChanged;

        public NodeInspectorPanel()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Width = 280;
            this.BackColor = BrandTheme.Surface;
            this.BorderStyle = BorderStyle.FixedSingle;

            _titleLabel = new Label
            {
                Text = "Node Inspector",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = BrandTheme.PrimaryDark
            };
            this.Controls.Add(_titleLabel);

            _propertiesPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 0,
                AutoScroll = true,
                Padding = new Padding(8)
            };
            _propertiesPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            _propertiesPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            this.Controls.Add(_propertiesPanel);
        }

        public void SetNode(NodeModel node)
        {
            _currentNode = node;
            RefreshProperties();
        }

        private void RefreshProperties()
        {
            _propertiesPanel.Controls.Clear();
            _propertiesPanel.RowCount = 0;

            if (_currentNode == null)
            {
                _titleLabel.Text = "Node Inspector";
                return;
            }

            _titleLabel.Text = $"Inspector: {_currentNode.Title}";

            AddPropertyRow("Type", _currentNode.Type, true);

            AddPropertyRow("X", _currentNode.Position.X.ToString("F1"), false, (value) =>
            {
                if (float.TryParse(value, out float x))
                {
                    _currentNode.Position = new PointF(x, _currentNode.Position.Y);
                    NodePropertyChanged?.Invoke(_currentNode);
                }
            });

            AddPropertyRow("Y", _currentNode.Position.Y.ToString("F1"), false, (value) =>
            {
                if (float.TryParse(value, out float y))
                {
                    _currentNode.Position = new PointF(_currentNode.Position.X, y);
                    NodePropertyChanged?.Invoke(_currentNode);
                }
            });

            if (_currentNode.Type == "Extrude")
            {
                var height = _currentNode.Settings.ContainsKey("Height") ? _currentNode.Settings["Height"] : "1.0";
                AddPropertyRow("Height", height, false, (value) =>
                {
                    _currentNode.Settings["Height"] = value;
                    NodePropertyChanged?.Invoke(_currentNode);
                });

                var polyline = _currentNode.Settings.ContainsKey("Polyline") ? _currentNode.Settings["Polyline"] : "Rectangle";
                AddPropertyRow("Polyline", polyline, false, (value) =>
                {
                    _currentNode.Settings["Polyline"] = value;
                    NodePropertyChanged?.Invoke(_currentNode);
                });
            }

            AddPropertyRow("Shadow Mode", _currentNode.IsShadowMode.ToString(), false, (value) =>
            {
                if (bool.TryParse(value, out bool shadowMode))
                {
                    _currentNode.IsShadowMode = shadowMode;
                    NodePropertyChanged?.Invoke(_currentNode);
                }
            });
        }

        private void AddPropertyRow(string label, string value, bool readOnly, Action<string> onChange = null)
        {
            var row = _propertiesPanel.RowCount;
            _propertiesPanel.RowCount++;

            var labelControl = new Label
            {
                Text = label,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                ForeColor = BrandTheme.Text,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            _propertiesPanel.Controls.Add(labelControl, 0, row);

            Control valueControl;
            if (readOnly)
            {
                valueControl = new Label
                {
                    Text = value,
                    Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                    ForeColor = BrandTheme.Text,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft,
                    BackColor = BrandTheme.GridLines
                };
            }
            else
            {
                var textBox = new TextBox
                {
                    Text = value,
                    Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                    Dock = DockStyle.Fill,
                    BorderStyle = BorderStyle.FixedSingle
                };

                if (onChange != null)
                {
                    textBox.TextChanged += (s, e) => onChange(textBox.Text);
                }

                valueControl = textBox;
            }

            _propertiesPanel.Controls.Add(valueControl, 1, row);
        }
    }
}
