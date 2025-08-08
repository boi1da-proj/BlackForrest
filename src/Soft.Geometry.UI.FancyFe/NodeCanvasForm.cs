using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Soft.Geometry.UI.FancyFe
{
    public class NodeCanvasForm : Form
    {
        private GraphModel _graph = new GraphModel();
        private readonly Panel _canvas;
        private readonly List<NodeControl> _nodeControls = new List<NodeControl>();
        private readonly NodeInspectorPanel _inspectorPanel;
        private readonly ListBox _logListBox;
        private readonly ShadowPreviewPanel _shadowBadge;
        private readonly Panel _viewportStub;
        private NodeControl _dragging;
        private Point _dragOffset;
        private NodeControl _selectedNode;
        private bool _isShadowMode = false;

        // Undo/Redo stacks
        private Stack<GraphModel> _undoStack = new Stack<GraphModel>();
        private Stack<GraphModel> _redoStack = new Stack<GraphModel>();

        public NodeCanvasForm()
        {
            this.Text = "Soft.Geometry — Node Canvas (David Rutten-inspired, Shadow Code UI)";
            this.Width = 1400;
            this.Height = 900;
            this.BackColor = BrandTheme.Background;
            this.KeyPreview = true;

            // Shadow preview badge at the top
            _shadowBadge = new ShadowPreviewPanel();
            _shadowBadge.SetShadowActive(_isShadowMode);
            _shadowBadge.Click += (s, e) => ToggleShadowMode();
            this.Controls.Add(_shadowBadge);

            // Top bar below the badge
            var topBar = new Panel
            {
                Height = 42,
                Dock = DockStyle.Top,
                BackColor = BrandTheme.Surface,
                BorderStyle = BorderStyle.FixedSingle
            };

            var saveBtn = CreateNeonButton("Save", BrandTheme.Green, 8, 6);
            saveBtn.Click += (s, e) => SaveGraph();
            topBar.Controls.Add(saveBtn);

            var loadBtn = CreateNeonButton("Load", BrandTheme.Blue, 116, 6);
            loadBtn.Click += (s, e) => LoadGraph();
            topBar.Controls.Add(loadBtn);

            this.Controls.Add(topBar);

            // Main split container
            var splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 1000
            };

            // Left panel hosts a sub-split: canvas (top) + viewport (bottom)
            var leftSplit = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 620
            };

            _canvas = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = BrandTheme.Background,
                AutoScroll = true,
                DoubleBuffered = true
            };
            _canvas.Paint += Canvas_Paint;
            leftSplit.Panel1.Controls.Add(_canvas);

            _viewportStub = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            var vpTitle = new Label
            {
                Text = "3D Viewport (stub)",
                Dock = DockStyle.Top,
                Height = 22,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                BackColor = BrandTheme.ShadowMode
            };
            _viewportStub.Controls.Add(vpTitle);
            leftSplit.Panel2.Controls.Add(_viewportStub);

            splitContainer.Panel1.Controls.Add(leftSplit);

            // Right panel for inspector and logs
            var rightPanel = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 300
            };

            _inspectorPanel = new NodeInspectorPanel();
            _inspectorPanel.NodePropertyChanged += OnNodePropertyChanged;
            rightPanel.Panel1.Controls.Add(_inspectorPanel);

            var logPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = BrandTheme.Surface,
                BorderStyle = BorderStyle.FixedSingle
            };

            var logLabel = new Label
            {
                Text = "Shadow Code Log",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = BrandTheme.ShadowMode,
                Dock = DockStyle.Top,
                Height = 25,
                TextAlign = ContentAlignment.MiddleCenter
            };
            logPanel.Controls.Add(logLabel);

            _logListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                BackColor = BrandTheme.Background,
                ForeColor = BrandTheme.Text,
                Font = new Font("Consolas", 8f)
            };
            logPanel.Controls.Add(_logListBox);

            rightPanel.Panel2.Controls.Add(logPanel);
            splitContainer.Panel2.Controls.Add(rightPanel);

            this.Controls.Add(splitContainer);

            // Handlers
            _canvas.MouseDown += Canvas_MouseDown;
            _canvas.MouseMove += Canvas_MouseMove;
            _canvas.MouseUp += Canvas_MouseUp;

            // Seed initial
            AddNodeTemplate("Extrude", new PointF(100, 100));
            LogMessage("Graph initialized with Extrude node");
        }

        private Button CreateNeonButton(string text, Color color, int x, int y)
        {
            return new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(96, 28),
                FlatStyle = FlatStyle.Flat,
                BackColor = color,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8f, FontStyle.Bold)
            };
        }

        private void ToggleShadowMode()
        {
            _isShadowMode = !_isShadowMode;
            _graph.IsShadowMode = _isShadowMode;
            _shadowBadge.SetShadowActive(_isShadowMode);
            foreach (var ctrl in _nodeControls) ctrl.UpdateShadowMode(_isShadowMode);
            LogMessage($"Shadow Mode: {(_isShadowMode ? "ON" : "OFF")}");
        }

        private void Canvas_Paint(object sender, PaintEventArgs e)
        {
            using (var gridPen = new Pen(BrandTheme.GridLines, 1f))
            {
                int gridSize = 20;
                for (int x = 0; x < _canvas.Width; x += gridSize) e.Graphics.DrawLine(gridPen, x, 0, x, _canvas.Height);
                for (int y = 0; y < _canvas.Height; y += gridSize) e.Graphics.DrawLine(gridPen, 0, y, _canvas.Width, y);
            }
        }

        private void AddNodeTemplate(string type, PointF location)
        {
            var node = new NodeModel
            {
                Type = type,
                Title = type,
                Id = Guid.NewGuid().ToString("N"),
                Position = location,
                Size = new SizeF(240, 120),
                IsShadowMode = _isShadowMode
            };

            switch (type)
            {
                case "Extrude":
                    node.Inputs = new List<InputSocket> { new InputSocket { Name = "Polyline2D" } };
                    node.Outputs = new List<OutputSocket> { new OutputSocket { Name = "Mesh3D" } };
                    node.Settings["Height"] = "1.0";
                    node.Settings["Polyline"] = "Rectangle";
                    break;
            }

            _graph.Nodes.Add(node);
            var ctrl = new NodeControl(node);
            ctrl.UpdateShadowMode(_isShadowMode);
            ctrl.MouseDown += (s, e) => { _dragging = ctrl; _dragOffset = new Point(e.X, e.Y); _selectedNode = ctrl; };
            ctrl.MouseDoubleClick += (s, e) => { _inspectorPanel.SetNode(ctrl.Model); };
            _nodeControls.Add(ctrl);
            _canvas.Controls.Add(ctrl);
        }

        private void OnNodePropertyChanged(NodeModel node)
        {
            var ctrl = _nodeControls.FirstOrDefault(n => n.Model.Id == node.Id);
            if (ctrl != null)
            {
                ctrl.UpdatePosition(node.Position);
                ctrl.UpdateShadowMode(node.IsShadowMode);
            }
            LogMessage($"Updated {node.Title} properties");
        }

        private void Canvas_MouseDown(object sender, MouseEventArgs e)
        {
            // no-op; node mouse handlers manage dragging start
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_dragging != null)
            {
                var newPos = new Point(e.X - _dragOffset.X, e.Y - _dragOffset.Y);
                _dragging.Location = newPos;
                _dragging.Model.Position = new PointF(newPos.X, newPos.Y);
            }
        }

        private void Canvas_MouseUp(object sender, MouseEventArgs e)
        {
            if (_dragging != null)
            {
                int snappedX = (int)(Math.Round(_dragging.Left / 10.0) * 10);
                int snappedY = (int)(Math.Round(_dragging.Top / 10.0) * 10);
                _dragging.Location = new Point(snappedX, snappedY);
                _dragging.Model.Position = new PointF(snappedX, snappedY);
                LogMessage($"Moved {_dragging.Model.Title} to ({snappedX}, {snappedY})");
                _dragging = null;
            }
        }

        private void LogMessage(string message)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            _logListBox.Items.Add($"[{timestamp}] {message}");
            _logListBox.SelectedIndex = _logListBox.Items.Count - 1;
        }
    }
}
