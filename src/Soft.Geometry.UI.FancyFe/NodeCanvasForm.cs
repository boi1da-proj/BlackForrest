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

        // Selection & interaction
        private readonly HashSet<NodeControl> _selectedNodes = new HashSet<NodeControl>();
        private NodeControl _dragging;
        private Point _dragOffset;
        private bool _isShadowMode = false;

        private bool _isSelecting = false;
        private Point _selectStart;
        private Rectangle _selectionRect;

        // Multidrag helpers
        private Dictionary<NodeControl, Point> _dragStartPositions = new Dictionary<NodeControl, Point>();

        // Undo/Redo stacks
        private Stack<GraphModel> _undoStack = new Stack<GraphModel>();
        private Stack<GraphModel> _redoStack = new Stack<GraphModel>();

        // Clipboard
        private List<NodeModel> _clipboard = new List<NodeModel>();

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
            // Top toolbar
            var topBar = new Panel 
            { 
                Height = 50, 
                Dock = DockStyle.Top, 
                BackColor = BrandTheme.Surface,
                BorderStyle = BorderStyle.FixedSingle
            };

            var saveBtn = CreateNeonButton("Save Graph", BrandTheme.Green, 8, 8);
            saveBtn.Click += (s, e) => SaveGraph();
            topBar.Controls.Add(saveBtn);

            var loadBtn = CreateNeonButton("Load Graph", BrandTheme.Blue, 120, 8);
            loadBtn.Click += (s, e) => LoadGraph();
            topBar.Controls.Add(loadBtn);

            var shadowToggleBtn = CreateNeonButton("Shadow", BrandTheme.ShadowMode, 232, 8);
            shadowToggleBtn.Click += (s, e) => ToggleShadowMode();
            topBar.Controls.Add(shadowToggleBtn);

            this.Controls.Add(topBar);

            // Main split container
            var splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 1050
            };

            // Left area (canvas + toolbox)
            var leftArea = new Panel { Dock = DockStyle.Fill };

            // Toolbox on the left
            var toolbox = new Panel
            {
                Dock = DockStyle.Left,
                Width = 160,
                BackColor = BrandTheme.Surface,
                BorderStyle = BorderStyle.FixedSingle
            };

            var toolboxHeader = new Label
            {
                Text = "Palette",
                Dock = DockStyle.Top,
                Height = 28,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = BrandTheme.Primary,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };
            toolbox.Controls.Add(toolboxHeader);

            // Node type buttons
            int by = 36;
            toolbox.Controls.Add(CreateToolboxButton("Extrude", BrandTheme.Primary, by, () => AddNodeTemplate("Extrude"))); by += 36;
            toolbox.Controls.Add(CreateToolboxButton("Loft", BrandTheme.NeonCyan, by, () => AddNodeTemplate("Loft"))); by += 36;
            toolbox.Controls.Add(CreateToolboxButton("Boolean", BrandTheme.Purple, by, () => AddNodeTemplate("Boolean"))); by += 36;
            toolbox.Controls.Add(CreateToolboxButton("Transform", BrandTheme.Yellow, by, () => AddNodeTemplate("Transform"))); by += 36;
            toolbox.Controls.Add(CreateToolboxButton("Fillet", BrandTheme.Green, by, () => AddNodeTemplate("Fillet"))); by += 36;

            leftArea.Controls.Add(toolbox);

            // Canvas area with grid
            _canvas = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = BrandTheme.Background,
                AutoScroll = true,
                DoubleBuffered = true
            };
            _canvas.Paint += Canvas_Paint;
            leftArea.Controls.Add(_canvas);

            splitContainer.Panel1.Controls.Add(leftArea);

            // Right panel for inspector and logs
            var rightPanel = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 320
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

        private Button CreateToolboxButton(string text, Color color, int y, Action onClick)
        {
            var b = new Button
            {
                Text = text,
                Location = new Point(10, y),
                Size = new Size(130, 28),
                FlatStyle = FlatStyle.Flat,
                BackColor = color,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8f, FontStyle.Bold)
            };
            b.Click += (s, e) => onClick();
            return b;
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
                        case Keys.C: CopySelected(); break;
                        case Keys.V: PasteClipboard(); break;
                        case Keys.D: DuplicateSelected(); break;
                    }
                }
                if (e.KeyCode == Keys.Delete) DeleteSelectedNodes();
            };
        }

        private void Canvas_Paint(object sender, PaintEventArgs e)
        {
            // Grid
            using (var gridPen = new Pen(BrandTheme.GridLines, 1f))
            {
                int gridSize = 20;
                for (int x = 0; x < _canvas.Width; x += gridSize)
                    e.Graphics.DrawLine(gridPen, x, 0, x, _canvas.Height);
                for (int y = 0; y < _canvas.Height; y += gridSize)
                    e.Graphics.DrawLine(gridPen, 0, y, _canvas.Width, y);
            }

            // Connections (simple center-to-center for now)
            using (var connPen = new Pen(BrandTheme.ConnectionLine, 2f))
            {
                foreach (var c in _graph.Connections)
                {
                    var src = _nodeControls.FirstOrDefault(n => n.Model.Id == c.SourceNodeId);
                    var dst = _nodeControls.FirstOrDefault(n => n.Model.Id == c.TargetNodeId);
                    if (src == null || dst == null) continue;
                    var p1 = new Point(src.Left + src.Width - 4, src.Top + src.Height / 2);
                    var p2 = new Point(dst.Left + 4, dst.Top + dst.Height / 2);
                    e.Graphics.DrawLine(connPen, p1, p2);
                }
            }

            // Selection rectangle
            if (_isSelecting)
            {
                using var selBrush = new SolidBrush(Color.FromArgb(40, BrandTheme.NeonCyan));
                using var selPen = new Pen(BrandTheme.NeonCyan, 1.5f);
                e.Graphics.FillRectangle(selBrush, _selectionRect);
                e.Graphics.DrawRectangle(selPen, _selectionRect);
            }
        }

        private void SeedInitialGraph()
        {
            AddNodeTemplate("Extrude", new PointF(220, 140));
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
                Size = new SizeF(240, 140),
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
                    node.Inputs = new List<InputSocket> { new InputSocket { Name = "Profile1" }, new InputSocket { Name = "Profile2" } };
                    node.Outputs = new List<OutputSocket> { new OutputSocket { Name = "Surface" } };
                    break;
                case "Boolean":
                    node.Inputs = new List<InputSocket> { new InputSocket { Name = "MeshA" }, new InputSocket { Name = "MeshB" } };
                    node.Outputs = new List<OutputSocket> { new OutputSocket { Name = "Result" } };
                    node.Settings["Operation"] = "Union";
                    break;
                case "Transform":
                    node.Inputs = new List<InputSocket> { new InputSocket { Name = "Mesh" } };
                    node.Outputs = new List<OutputSocket> { new OutputSocket { Name = "Mesh" } };
                    node.Settings["Translate"] = "0,0,0";
                    break;
                case "Fillet":
                    node.Inputs = new List<InputSocket> { new InputSocket { Name = "Polyline2D" } };
                    node.Outputs = new List<OutputSocket> { new OutputSocket { Name = "Polyline2D" } };
                    node.Settings["Radius"] = "0.25";
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
                    // Toggle selection on Ctrl, otherwise select solely
                    if ((ModifierKeys & Keys.Control) == Keys.Control)
                    {
                        ToggleSelection(ctrl);
                    }
                    else
                    {
                        SelectSingle(ctrl);
                    }

                    // Setup drag for all selected nodes
                    _dragging = ctrl;
                    _dragOffset = new Point(e.X, e.Y);
                    _dragStartPositions = _selectedNodes.ToDictionary(n => n, n => n.Location);
                }
            };

            ctrl.MouseDoubleClick += (s, e) =>
            {
                SelectSingle(ctrl);
                _inspectorPanel.SetNode(ctrl.Model);
            };

            _nodeControls.Add(ctrl);
            _canvas.Controls.Add(ctrl);
            ctrl.BringToFront();

            LogMessage($"Added {type} node at ({pos.X:F0}, {pos.Y:F0})");
        }

        private void SelectNone()
        {
            foreach (var n in _selectedNodes)
                n.UpdateSelection(false);
            _selectedNodes.Clear();
            _inspectorPanel.SetNode(null);
        }

        private void SelectSingle(NodeControl node)
        {
            SelectNone();
            _selectedNodes.Add(node);
            node.UpdateSelection(true);
            _inspectorPanel.SetNode(node.Model);
        }

        private void ToggleSelection(NodeControl node)
        {
            if (_selectedNodes.Contains(node))
            {
                node.UpdateSelection(false);
                _selectedNodes.Remove(node);
            }
            else
            {
                node.UpdateSelection(true);
                _selectedNodes.Add(node);
                _inspectorPanel.SetNode(node.Model);
            }
        }

        private void ToggleShadowMode()
        {
            _isShadowMode = !_isShadowMode;
            _graph.IsShadowMode = _isShadowMode;

            foreach (var ctrl in _nodeControls)
                ctrl.UpdateShadowMode(_isShadowMode);

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
                _canvas.Controls.Remove(ctrl);
            _nodeControls.Clear();
            SelectNone();

            // Recreate controls from graph
            foreach (var node in _graph.Nodes)
            {
                var ctrl = new NodeControl(node);
                ctrl.UpdateShadowMode(_isShadowMode);
                _nodeControls.Add(ctrl);
                _canvas.Controls.Add(ctrl);

                // Reattach handlers
                ctrl.MouseDown += (s, e) =>
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        if ((ModifierKeys & Keys.Control) == Keys.Control) ToggleSelection(ctrl);
                        else SelectSingle(ctrl);
                        _dragging = ctrl;
                        _dragOffset = new Point(e.X, e.Y);
                        _dragStartPositions = _selectedNodes.ToDictionary(n => n, n => n.Location);
                    }
                };
                ctrl.MouseDoubleClick += (s, e) => { SelectSingle(ctrl); _inspectorPanel.SetNode(ctrl.Model); };
            }

            _canvas.Invalidate();
        }

        private void DeleteSelectedNodes()
        {
            if (_selectedNodes.Count == 0) return;
            SaveSnapshot();
            foreach (var ctrl in _selectedNodes.ToList())
            {
                _graph.Nodes.Remove(ctrl.Model);
                _canvas.Controls.Remove(ctrl);
                _nodeControls.Remove(ctrl);
            }
            _selectedNodes.Clear();
            _inspectorPanel.SetNode(null);
            LogMessage("Deleted selected nodes");
            _canvas.Invalidate();
        }

        private void CopySelected()
        {
            _clipboard.Clear();
            foreach (var ctrl in _selectedNodes)
            {
                var m = ctrl.Model;
                var copy = new NodeModel
                {
                    Id = Guid.NewGuid().ToString("N"),
                    Type = m.Type,
                    Title = m.Title,
                    Position = new PointF(m.Position.X + 20, m.Position.Y + 20),
                    Size = m.Size,
                    IsShadowMode = m.IsShadowMode,
                    Settings = new Dictionary<string, string>(m.Settings),
                    Inputs = new List<InputSocket>(m.Inputs.Select(i => new InputSocket { Name = i.Name, Type = i.Type })),
                    Outputs = new List<OutputSocket>(m.Outputs.Select(o => new OutputSocket { Name = o.Name, Type = o.Type }))
                };
                _clipboard.Add(copy);
            }
            LogMessage($"Copied {_clipboard.Count} node(s)");
        }

        private void PasteClipboard()
        {
            if (_clipboard.Count == 0) return;
            SaveSnapshot();
            SelectNone();
            foreach (var copied in _clipboard)
            {
                // Create a fresh copy per paste operation
                var node = new NodeModel
                {
                    Id = Guid.NewGuid().ToString("N"),
                    Type = copied.Type,
                    Title = copied.Title,
                    Position = new PointF(copied.Position.X + 20, copied.Position.Y + 20),
                    Size = copied.Size,
                    IsShadowMode = copied.IsShadowMode,
                    Settings = new Dictionary<string, string>(copied.Settings),
                    Inputs = new List<InputSocket>(copied.Inputs.Select(i => new InputSocket { Name = i.Name, Type = i.Type })),
                    Outputs = new List<OutputSocket>(copied.Outputs.Select(o => new OutputSocket { Name = o.Name, Type = o.Type }))
                };
                _graph.Nodes.Add(node);
                var ctrl = new NodeControl(node);
                ctrl.UpdateShadowMode(_isShadowMode);
                _nodeControls.Add(ctrl);
                _canvas.Controls.Add(ctrl);
                _selectedNodes.Add(ctrl);
                ctrl.UpdateSelection(true);

                // Handlers
                ctrl.MouseDown += (s, e) =>
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        if ((ModifierKeys & Keys.Control) == Keys.Control) ToggleSelection(ctrl);
                        else SelectSingle(ctrl);
                        _dragging = ctrl;
                        _dragOffset = new Point(e.X, e.Y);
                        _dragStartPositions = _selectedNodes.ToDictionary(n => n, n => n.Location);
                    }
                };
                ctrl.MouseDoubleClick += (s, e) => { SelectSingle(ctrl); _inspectorPanel.SetNode(ctrl.Model); };
            }
            LogMessage($"Pasted {_clipboard.Count} node(s)");
            _canvas.Invalidate();
        }

        private void DuplicateSelected()
        {
            CopySelected();
            PasteClipboard();
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
                    Inputs = new List<InputSocket>(n.Inputs.Select(i => new InputSocket { Name = i.Name, Type = i.Type })),
                    Outputs = new List<OutputSocket>(n.Outputs.Select(o => new OutputSocket { Name = o.Name, Type = o.Type }))
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
                // If clicked empty space, start selection rectangle
                var hit = _nodeControls.FirstOrDefault(n => n.Bounds.Contains(e.Location));
                if (hit == null)
                {
                    _isSelecting = true;
                    _selectStart = e.Location;
                    _selectionRect = new Rectangle(e.Location, Size.Empty);
                    _canvas.Invalidate();
                }
            }
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_dragging != null && _selectedNodes.Count > 0)
            {
                // Move all selected nodes relative to their start
                foreach (var kv in _dragStartPositions)
                {
                    var ctrl = kv.Key;
                    var start = kv.Value;
                    var newPos = new Point(
                        start.X + (e.X - _dragOffset.X),
                        start.Y + (e.Y - _dragOffset.Y));
                    ctrl.Location = newPos;
                    ctrl.Model.Position = new PointF(newPos.X, newPos.Y);
                }
                _canvas.Invalidate();
            }
            else if (_isSelecting)
            {
                int x = Math.Min(_selectStart.X, e.X);
                int y = Math.Min(_selectStart.Y, e.Y);
                int w = Math.Abs(e.X - _selectStart.X);
                int h = Math.Abs(e.Y - _selectStart.Y);
                _selectionRect = new Rectangle(x, y, w, h);
                _canvas.Invalidate();
            }
        }

        private void Canvas_MouseUp(object sender, MouseEventArgs e)
        {
            if (_dragging != null)
            {
                // Snap to grid on release
                foreach (var ctrl in _selectedNodes)
                {
                    int snappedX = (int)(Math.Round(ctrl.Left / 10.0) * 10);
                    int snappedY = (int)(Math.Round(ctrl.Top / 10.0) * 10);
                    ctrl.Location = new Point(snappedX, snappedY);
                    ctrl.Model.Position = new PointF(snappedX, snappedY);
                }
                LogMessage("Moved node(s) with snapping");
                _dragging = null;
                _dragStartPositions.Clear();
                _canvas.Invalidate();
                return;
            }

            if (_isSelecting)
            {
                SelectNone();
                foreach (var n in _nodeControls)
                {
                    if (_selectionRect.IntersectsWith(n.Bounds))
                    {
                        _selectedNodes.Add(n);
                        n.UpdateSelection(true);
                    }
                }
                _isSelecting = false;
                _selectionRect = Rectangle.Empty;
                _canvas.Invalidate();
            }
        }
    }
}
