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
        private readonly NLToGraphPanel _nlPanel;
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
            this.Width = 1500;
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
                SplitterDistance = 1120
            };

            // Left panel hosts a sub-split: canvas (top) + viewport (bottom)
            var leftSplit = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 340
            };

            _nlPanel = new NLToGraphPanel(_graph);
            leftSplit.Panel1.Controls.Add(_nlPanel);

            var innerVertSplit = new SplitContainer
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
            innerVertSplit.Panel1.Controls.Add(_canvas);

            _viewportStub = new Viewport3DPanel();
            innerVertSplit.Panel2.Controls.Add(_viewportStub);

            leftSplit.Panel2.Controls.Add(innerVertSplit);
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
            AddNodeTemplate("Extrude", new PointF(420, 140));
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
                case "Loft":
                    node.Inputs = new List<InputSocket> { new InputSocket { Name = "Profile1" }, new InputSocket { Name = "Profile2" } };
                    node.Outputs = new List<OutputSocket> { new OutputSocket { Name = "Surface" } };
                    break;
                case "Transform":
                    node.Inputs = new List<InputSocket> { new InputSocket { Name = "Mesh" } };
                    node.Outputs = new List<OutputSocket> { new OutputSocket { Name = "Mesh" } };
                    node.Settings["Translate"] = "0,0,0";
                    break;
                case "Boolean":
                    node.Inputs = new List<InputSocket> { new InputSocket { Name = "MeshA" }, new InputSocket { Name = "MeshB" } };
                    node.Outputs = new List<OutputSocket> { new OutputSocket { Name = "Result" } };
                    node.Settings["Operation"] = "Union";
                    break;
                case "Fillet":
                    node.Inputs = new List<InputSocket> { new InputSocket { Name = "Polyline2D" } };
                    node.Outputs = new List<OutputSocket> { new OutputSocket { Name = "Polyline2D" } };
                    node.Settings["Radius"] = "0.25";
                    break;
                case "Loft-by-Path":
                    node.Inputs = new List<InputSocket> { new InputSocket { Name = "Path" }, new InputSocket { Name = "Profiles" } };
                    node.Outputs = new List<OutputSocket> { new OutputSocket { Name = "Surface" } };
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

        private void Canvas_MouseDown(object sender, MouseEventArgs e) { }

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

        private void SaveGraph()
        {
            using var saveDialog = new SaveFileDialog
            {
                Filter = "Graph files (*.json)|*.json|All files (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true
            };
            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    GraphSerializer.Save(saveDialog.FileName, _graph);
                    LogMessage($"Graph saved to {saveDialog.FileName}");
                }
                catch (Exception ex)
                {
                    LogMessage($"Error saving graph: {ex.Message}");
                }
            }
        }

        private void LoadGraph()
        {
            using var openDialog = new OpenFileDialog
            {
                Filter = "Graph files (*.json)|*.json|All files (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true
            };
            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    _graph = GraphSerializer.Load(openDialog.FileName);
                    RefreshCanvas();
                    LogMessage($"Graph loaded from {openDialog.FileName}");
                }
                catch (Exception ex)
                {
                    LogMessage($"Error loading graph: {ex.Message}");
                }
            }
        }

        private void RefreshCanvas()
        {
            foreach (var ctrl in _nodeControls) _canvas.Controls.Remove(ctrl);
            _nodeControls.Clear();
            foreach (var node in _graph.Nodes)
            {
                var ctrl = new NodeControl(node);
                ctrl.UpdateShadowMode(_isShadowMode);
                _nodeControls.Add(ctrl);
                _canvas.Controls.Add(ctrl);
                ctrl.MouseDown += (s, e) => { _dragging = ctrl; _dragOffset = new Point(e.X, e.Y); _selectedNode = ctrl; };
                ctrl.MouseDoubleClick += (s, e) => { _inspectorPanel.SetNode(ctrl.Model); };
            }
            _canvas.Invalidate();
        }

        private void LogMessage(string message)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            _logListBox.Items.Add($"[{timestamp}] {message}");
            _logListBox.SelectedIndex = _logListBox.Items.Count - 1;
        }
    }
}
