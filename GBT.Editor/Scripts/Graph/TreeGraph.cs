using GBT;
using GBT.Nodes;
using Godot;
using Godot.Collections;
using Newtonsoft.Json;
using System.Linq;

public partial class TreeGraph : GraphEdit {
    private TreeGraphContextMenu? _contextMenu;
    private Dictionary<int, Callable> _contextActions = new();
    private BehaviorTree _tree = new();

    private bool _shouldRefreshConnections = false;
    private bool _shouldUpdateJsonOutput = false;

    [Export] public RenameNodeModal? RenameNodeModal { get; private set; }
    [Export] public TextEdit? JsonOutput { get; private set; }

    public event System.Action? RootChanged;

    public GBTNode? RootNode {
        get => _tree.RootNode;
        set {
            if (value == null) return;
            _tree.SetRootNode(value);
            RootChanged?.Invoke();
        }
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        InitializeContextMenu();
        PopupRequest += OnRequestContextMenu;
        ConnectionRequest += OnConnectionRequest;
        ConnectionToEmpty += OnConnectionToEmpty;
        DeleteNodesRequest += OnDeleteNodesRequest;
        ChildEnteredTree += (_) => UpdateJsonOutput();
        ChildExitingTree += (_) => UpdateJsonOutput();

        // Clean up all temporary data
        foreach (Node? child in FindChildren("*", "TreeGraphNode")) {
            RemoveChild(child);
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) {
        if (_shouldRefreshConnections) {
            _shouldRefreshConnections = false;
            DoRefreshConnections();
        }
        if (_shouldUpdateJsonOutput) {
            _shouldUpdateJsonOutput = false;
            DoUpdateJsonOutput();
        }
    }

    private void OnRequestContextMenu(Vector2 pos) {
        if (_contextMenu != null) {
            _contextMenu.Position = new Vector2I((int)pos.X, (int)pos.Y);
            _contextMenu.Show();
        }
    }

    private void OnConnectionRequest(StringName fromNode, long fromPort, StringName toNode, long toPort) {
        TreeGraphNode? sourceNode = FindGraphNode(fromNode);
        if (sourceNode == null) return;

        if (sourceNode.RequestSlotConnection(fromPort, toNode, toPort)) {
            RefreshConnections();
        }
    }

    private void OnConnectionToEmpty(StringName fromNode, long fromPort, Vector2 releasePosition) {
        TreeGraphNode? sourceNode = FindGraphNode(fromNode);
        if (sourceNode == null) return;
        if (sourceNode.RequestSlotConnection(fromPort, string.Empty, -1)) {
            RefreshConnections();
        }
    }

    private void OnDeleteNodesRequest(Array nodes) {
        nodes.Select(nodeName => FindGraphNode((string)nodeName)).Where(node => node != null).ToList().ForEach(node => {
            node!.Delete();
        });
    }

    private void InitializeContextMenu() {
        _contextMenu = GetNode<TreeGraphContextMenu>("ContextMenu");
        _contextMenu?.AddMenuItems([
            new(0, "Create test nodes", CreateTestNode),
        ]);
    }

    private void CreateTestNode() {
        SequenceNode testRoot = new SequenceNode("Seq.").AddChildren(
            new CallbackNode("cb1"),
            new CallbackNode("cb2"),
            new CallbackNode("cb3")
        );
        var rootGraphNode = new TreeGraphNode() {
            PositionOffset = (GetViewport().GetMousePosition() + ScrollOffset) / Zoom,
            DataNode = testRoot,
        };
        AddChild(rootGraphNode);
        var i = 0;
        foreach (GBTNode testChild in testRoot.Children) {
            var childGraphNode = new TreeGraphNode() {
                DataNode = testChild,
            };
            AddChild(childGraphNode);
            i++;
        }
        if (RootNode == null) {
            RootNode = testRoot;
        }
        RefreshConnections();
    }

    public void CreateGraphNode(GBTNode dataNode) {
        var node = new TreeGraphNode() {
            PositionOffset = (GetViewport().GetMousePosition() + ScrollOffset) / Zoom,
            DataNode = dataNode,
        };
        AddChild(node);
        if (RootNode == null) {
            RootNode = dataNode;
        }
        RefreshConnections();
    }

    public TreeGraphNode? FindGraphNode(string nodeName) {
        if (string.IsNullOrEmpty(nodeName)) return null;
        return GetNodeOrNull<TreeGraphNode>(nodeName);
    }

    public void RefreshConnections() {
        _shouldRefreshConnections = true;
    }

    private void DoRefreshConnections() {
        ClearConnections();
        foreach (TreeGraphNode graphNode in GetChildren().Where(child => child is TreeGraphNode node && node.Drawer != null).Cast<TreeGraphNode>()) {
            if (graphNode.DataNode == null) continue;
            foreach (SlotConnection conn in graphNode.Drawer!.GetSlotConnections()
                .Where(conn => !string.IsNullOrEmpty(conn.FromNode) && !string.IsNullOrEmpty(conn.ToNode))) {
                ConnectNode(conn.FromNode, conn.FromPort, conn.ToNode, conn.ToPort);
            }
        }
        UpdateJsonOutput();
    }

    public void UpdateJsonOutput() {
        _shouldUpdateJsonOutput = true;
    }

    private void DoUpdateJsonOutput() {
        if (JsonOutput == null) return;
        if (_tree.RootNode == null) {
            JsonOutput.Text = "(no root node)";
            return;
        }
        var json = _tree.SaveAsJson();
        json = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(json), Formatting.Indented);
        JsonOutput.Text = json;

    }
}
