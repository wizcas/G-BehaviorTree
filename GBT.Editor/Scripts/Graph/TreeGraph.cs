using GBT;
using GBT.Nodes;
using Godot;
using Godot.Collections;
using Newtonsoft.Json;
using System.Linq;


public partial class TreeGraph : GraphEdit {
    private TreeGraphContextMenu? _contextMenu;
    private Dictionary<int, Callable> _contextActions = new();
    private BehaviorTree Tree => GraphManager.GetInstance().EditingTree;

    private bool _shouldRefreshConnections = false;
    private bool _shouldUpdateJsonOutput = false;

    [Export] public RenameNodeModal? RenameNodeModal { get; private set; }
    [Export] public TextEdit? JsonOutput { get; private set; }

    public event System.Action? RootChanged;

    public GBTNode? RootNode {
        get => Tree.RootNode;
        set {
            if (value == Tree.RootNode) return;
            Tree.SetRootNode(value);
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

        GraphManager.GetInstance().RequestReloadTree += Reload;
        Reload();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) {
        if (_shouldRefreshConnections) {
            _shouldRefreshConnections = false;
            ExecuteRefreshConnections();
        }
        if (_shouldUpdateJsonOutput) {
            _shouldUpdateJsonOutput = false;
            ExecuteUpdateJsonOutput();
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

    private void OnDeleteNodesRequest(Array deletingNodeIDs) {
        var needNewRootNode = deletingNodeIDs.Any((n) => (string)n == Tree.RootNode?.ID);
        if (needNewRootNode && Tree.RootNode != null) {
            var newRootID = Tree.Flatten().Select(n => n.ID).Except(deletingNodeIDs.Select(variant => variant.AsString())).FirstOrDefault();
            if (!string.IsNullOrEmpty(newRootID)) {
                Tree.SetRootNode(Tree.FindNode(newRootID));
            }
        }
        deletingNodeIDs.Select(nodeName => FindGraphNode((string)nodeName)).Where(node => node != null).ToList().ForEach(node => {
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
                PositionOffset = rootGraphNode.PositionOffset + new Vector2(400, (i - 1) * 200),
            };
            AddChild(childGraphNode);
            i++;
        }
        if (RootNode == null) {
            RootNode = testRoot;
        }
        RefreshConnections();
    }

    public void CreateGraphNode(GBTNode dataNode, bool skipRefresh = false) {
        var node = new TreeGraphNode() {
            PositionOffset = (GetViewport().GetMousePosition() + ScrollOffset) / Zoom,
            DataNode = dataNode,
        };
        AddChild(node);
        if (RootNode == null) {
            RootNode = dataNode;
        }
        if (!skipRefresh) {
            RefreshConnections();
        }
    }

    public void Reload() {
        Tree.RootNodeChanged += (tree) => RootChanged?.Invoke();
        TreeGraphNode[] nodes = GetChildren().Where(child => child is TreeGraphNode).Cast<TreeGraphNode>().ToArray();
        foreach (TreeGraphNode? node in nodes) {
            RemoveChild(node);
            node.Free();
        }
        foreach (GBTNode treeNode in Tree.Flatten()) {
            CreateGraphNode(treeNode, skipRefresh: true);
        }
        RefreshConnections();
        Callable.From(ArrangeNodes).CallDeferred();
    }

    public TreeGraphNode? FindGraphNode(string nodeName) {
        if (string.IsNullOrEmpty(nodeName)) return null;
        return GetNodeOrNull<TreeGraphNode>(nodeName);
    }

    public void RefreshConnections() {
        _shouldRefreshConnections = true;
    }

    private void ExecuteRefreshConnections() {
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

    private void ExecuteUpdateJsonOutput() {
        if (JsonOutput == null) return;
        if (Tree.RootNode == null) {
            JsonOutput.Text = "(no root node)";
            return;
        }
        var json = Tree.SaveAsJson();
        json = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(json), Formatting.Indented);
        JsonOutput.Text = json;

    }
}
