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

            SetupLayout();
            SetupEventHandlers();
            SeedInitialGraph();
        }

        private void SetupLayout()
        {
            // Top toolbar with neon styling
            var topBar = new Panel 
            { 
                Height = 50, 
                Dock = DockStyle.Top, 
                BackColor = BrandTheme.Surface,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Node creation buttons
            var addExtrudeBtn = CreateNeonButton("Add Extrude", BrandTheme.Primary, 8, 8);
            addExtrudeBtn.Click += (s, e) => AddNodeTemplate("Extrude");
            topBar.Controls.Add(addExtrudeBtn);

            var addLoftBtn = CreateNeonButton("Add Loft", BrandTheme.NeonCyan, 120, 8);
            addLoftBtn.Click += (s, e) => AddNodeTemplate("Loft");
            topBar.Controls.Add(addLoftBtn);

            var addBooleanBtn = CreateNeonButton("Add Boolean", BrandTheme.Purple, 232, 8);
            addBooleanBtn.Click += (s, e) => AddNodeTemplate("Boolean");
            topBar.Controls.Add(addBooleanBtn);

            // Shadow mode toggle
            var shadowToggleBtn = CreateNeonButton("Shadow Mode: OFF", BrandTheme.ShadowMode, 344, 8);
            shadowToggleBtn.Click += (s, e) => ToggleShadowMode();
            topBar.Controls.Add(shadowToggleBtn);

            // Save/Load buttons
            var saveBtn = CreateNeonButton("Save Graph", BrandTheme.Green, 456, 8);
            saveBtn.Click += (s, e) => SaveGraph();
            topBar.Controls.Add(saveBtn);

            var loadBtn = CreateNeonButton("Load Graph", BrandTheme.Blue, 568, 8);
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

            // Canvas area with grid
            _canvas = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = BrandTheme.Background,
                AutoScroll = true,
                DoubleBuffered = true
            };
            _canvas.Paint += Canvas_Paint;
            splitContainer.Panel1.Controls.Add(_canvas);

            // Right panel for inspector and logs
            var rightPanel = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 300
            };

            // Inspector panel
            _inspectorPanel = new NodeInspectorPanel();
            _inspectorPanel.NodePropertyChanged += OnNodePropertyChanged;
            rightPanel.Panel1.Controls.Add(_inspectorPanel);

            // Log panel
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
        }

        private Button CreateNeonButton(string text, Color color, int x, int y)
        {
            return new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(100, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = color,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8f, FontStyle.Bold)
            };
        }

        private void SetupEventHandlers()
        {
            // Canvas mouse events
            _canvas.MouseDown += Canvas_MouseDown;
            _canvas.MouseMove += Canvas_MouseMove;
            _canvas.MouseUp += Canvas_MouseUp;

            // Keyboard shortcuts
            this.KeyDown += (s, e) =>
            {
                if (e.Control)
                {
                    switch (e.KeyCode)
                    {
                        case Keys.Z: Undo(); break;
                        case Keys.Y: Redo(); break;
                        case Keys.S: SaveGraph(); break;
                        case Keys.O: LoadGraph(); break;
                        case Keys.Delete: DeleteSelectedNode(); break;
                    }
                }
            };
        }

        private void Canvas_Paint(object sender, PaintEventArgs e)
        {
            // Draw subtle grid
            using (var gridPen = new Pen(BrandTheme.GridLines, 1f))
            {
                int gridSize = 20;
                for (int x = 0; x < _canvas.Width; x += gridSize)
                {
                    e.Graphics.DrawLine(gridPen, x, 0, x, _canvas.Height);
                }
                for (int y = 0; y < _canvas.Height; y += gridSize)
                {
                    e.Graphics.DrawLine(gridPen, 0, y, _canvas.Width, y);
                }
            }
        }

        private void SeedInitialGraph()
        {
            AddNodeTemplate("Extrude", new PointF(100, 100));
            LogMessage("Graph initialized with Extrude node");
        }

        private void AddNodeTemplate(string type, PointF? location = null)
        {
            SaveSnapshot();

            var pos = location ?? new PointF(20f + _nodeControls.Count * 30, 80f + _nodeControls.Count * 20);
            var node = new NodeModel
            {
                Type = type,
                Title = type,
                Id = Guid.NewGuid().ToString("N"),
                Position = pos,
                Size = new SizeF(240, 120),
                IsShadowMode = _isShadowMode
            };

            // Configure node-specific inputs/outputs
            switch (type)
            {
                case "Extrude":
                    node.Inputs = new List<InputSocket> { new InputSocket { Name = "Polyline2D" } };
                    node.Outputs = new List<OutputSocket> { new OutputSocket { Name = "Mesh3D" } };
                    node.Settings["Height"] = "1.0";
                    node.Settings["Polyline"] = "Rectangle";
                    break;
                case "Loft":
                    node.Inputs = new List<InputSocket> 
                    { 
                        new InputSocket { Name = "Profile1" },
                        new InputSocket { Name = "Profile2" }
                    };
                    node.Outputs = new List<OutputSocket> { new OutputSocket { Name = "Surface" } };
                    break;
                case "Boolean":
                    node.Inputs = new List<InputSocket> 
                    { 
                        new InputSocket { Name = "MeshA" },
                        new InputSocket { Name = "MeshB" }
                    };
                    node.Outputs = new List<OutputSocket> { new OutputSocket { Name = "Result" } };
                    node.Settings["Operation"] = "Union";
                    break;
            }

            _graph.Nodes.Add(node);
            var ctrl = new NodeControl(node);
            ctrl.UpdateShadowMode(_isShadowMode);

            // Wire up node events
            ctrl.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    _dragging = ctrl;
                    _dragOffset = new Point(e.X, e.Y);
                    SelectNode(ctrl);
                }
            };

            ctrl.MouseDoubleClick += (s, e) =>
            {
                SelectNode(ctrl);
                _inspectorPanel.SetNode(ctrl.Model);
            };

            _nodeControls.Add(ctrl);
            _canvas.Controls.Add(ctrl);
            ctrl.BringToFront();

            LogMessage($"Added {type} node at ({pos.X:F0}, {pos.Y:F0})");
        }

        private void SelectNode(NodeControl node)
        {
            // Deselect previous
            if (_selectedNode != null)
            {
                _selectedNode.UpdateSelection(false);
            }

            _selectedNode = node;
            if (_selectedNode != null)
            {
                _selectedNode.UpdateSelection(true);
                _inspectorPanel.SetNode(_selectedNode.Model);
            }
        }

        private void ToggleShadowMode()
        {
            _isShadowMode = !_isShadowMode;
            _graph.IsShadowMode = _isShadowMode;

            foreach (var ctrl in _nodeControls)
            {
                ctrl.UpdateShadowMode(_isShadowMode);
            }

            LogMessage($"Shadow Mode: {(_isShadowMode ? "ON" : "OFF")}");
        }

        private void SaveGraph()
        {
            using (var saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "Graph files (*.json)|*.json|All files (*.*)|*.*";
                saveDialog.FilterIndex = 1;
                saveDialog.RestoreDirectory = true;

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
        }

        private void LoadGraph()
        {
            using (var openDialog = new OpenFileDialog())
            {
                openDialog.Filter = "Graph files (*.json)|*.json|All files (*.*)|*.*";
                openDialog.FilterIndex = 1;
                openDialog.RestoreDirectory = true;

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
        }

        private void RefreshCanvas()
        {
            // Clear existing controls
            foreach (var ctrl in _nodeControls)
            {
                _canvas.Controls.Remove(ctrl);
            }
            _nodeControls.Clear();

            // Recreate controls from graph
            foreach (var node in _graph.Nodes)
            {
                var ctrl = new NodeControl(node);
                ctrl.UpdateShadowMode(_isShadowMode);
                _nodeControls.Add(ctrl);
                _canvas.Controls.Add(ctrl);
            }
        }

        private void DeleteSelectedNode()
        {
            if (_selectedNode != null)
            {
                SaveSnapshot();
                _graph.Nodes.Remove(_selectedNode.Model);
                _nodeControls.Remove(_selectedNode);
                _canvas.Controls.Remove(_selectedNode);
                _selectedNode.Dispose();
                _selectedNode = null;
                _inspectorPanel.SetNode(null);
                LogMessage("Selected node deleted");
            }
        }

        private void OnNodePropertyChanged(NodeModel node)
        {
            // Update the corresponding control
            var ctrl = _nodeControls.FirstOrDefault(n => n.Model.Id == node.Id);
            if (ctrl != null)
            {
                ctrl.UpdatePosition(node.Position);
                ctrl.UpdateShadowMode(node.IsShadowMode);
            }
            LogMessage($"Updated {node.Title} properties");
        }

        private void LogMessage(string message)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            _logListBox.Items.Add($"[{timestamp}] {message}");
            _logListBox.SelectedIndex = _logListBox.Items.Count - 1;
        }

        // Undo/Redo implementation
        private void SaveSnapshot()
        {
            _undoStack.Push(CloneGraph(_graph));
            _redoStack.Clear();
        }

        private GraphModel CloneGraph(GraphModel g)
        {
            return new GraphModel
            {
                Name = g.Name,
                Version = g.Version,
                IsShadowMode = g.IsShadowMode,
                Nodes = g.Nodes.Select(n => new NodeModel
                {
                    Id = n.Id,
                    Type = n.Type,
                    Title = n.Title,
                    Position = n.Position,
                    Size = n.Size,
                    IsShadowMode = n.IsShadowMode,
                    Settings = new Dictionary<string, string>(n.Settings),
                    Inputs = new List<InputSocket>(n.Inputs),
                    Outputs = new List<OutputSocket>(n.Outputs)
                }).ToList(),
                Connections = new List<ConnectionModel>(g.Connections)
            };
        }

        private void Undo()
        {
            if (_undoStack.Count > 0)
            {
                _redoStack.Push(CloneGraph(_graph));
                _graph = _undoStack.Pop();
                RefreshCanvas();
                LogMessage("Undo performed");
            }
        }

        private void Redo()
        {
            if (_redoStack.Count > 0)
            {
                _undoStack.Push(CloneGraph(_graph));
                _graph = _redoStack.Pop();
                RefreshCanvas();
                LogMessage("Redo performed");
            }
        }

        // Mouse event handlers
        private void Canvas_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // Check if clicking on empty space
                var hitNode = _nodeControls.FirstOrDefault(n => 
                    n.Bounds.Contains(e.Location));
                
                if (hitNode == null)
                {
                    SelectNode(null);
                }
            }
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
                LogMessage($"Moved {_dragging.Model.Title} to ({_dragging.Model.Position.X:F0}, {_dragging.Model.Position.Y:F0})");
                _dragging = null;
            }
        }
    }
}
