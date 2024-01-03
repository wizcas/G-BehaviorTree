using GBT.Nodes;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

[GlobalClass]
public partial class TreeGraphNode : GraphNode {
    public TreeGraph? Graph { get; private set; }

    private GBTNode? _dataNode;
    public GBTNode? DataNode {
        get => _dataNode;
        set {
            if (_dataNode == value) return;
            GBTNode? oldNode = _dataNode;
            _dataNode = value;
            UpdateNode(oldNode?.GetType() == _dataNode?.GetType());
        }
    }
    public GBTNodeDrawer? Drawer { get; private set; }

    private static Dictionary<Type, Func<TreeGraphNode, GBTNodeDrawer>> NodeDrawerMap = new() {
        { typeof(SequenceNode), (graphNode)=>new ListCompositeNodeDrawer(graphNode) },
        { typeof(SelectorNode), (graphNode)=>new ListCompositeNodeDrawer(graphNode) },
    };

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        Graph = GetNode<TreeGraph>("..");
        Resizable = true;
        GetTitlebarHBox().GuiInput += (e) => {
            if (e is InputEventMouseButton mouseEvent && mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.DoubleClick) {
                ShowRenameModal();
            }
        };
        var renameButton = new Button() { Text = "Re" }; // TODO: icon
        renameButton.Pressed += ShowRenameModal;
        GetTitlebarHBox().AddChild(renameButton);
        if (Graph.RenameNodeModal != null) {
            Graph.RenameNodeModal.NameChanged += OnNodeNameChanged;
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) {
    }

    private void UpdateNode(bool shouldUpdateDrawer) {
        ClearAllSlots();

        Name = DataNode?.ID ?? "EmptyGraphNode";
        Title = DataNode?.Name ?? "(No Node)";

        if (DataNode == null) return;
        if (Drawer == null || shouldUpdateDrawer) {
            // Create graph slot drawer by GBT Node types
            if (NodeDrawerMap.TryGetValue(DataNode.GetType(), out Func<TreeGraphNode, GBTNodeDrawer>? drawerFactory)) {
                Drawer = drawerFactory(this);
            } else {
                Drawer = new UnknownNodeDrawer(this);
            }
        }
        Drawer.DrawSlots(DataNode);
    }

    private void ShowRenameModal() {
        Callable.From(() => {
            Graph?.RenameNodeModal?.Show(this);
        }).CallDeferred();
    }

    private void OnNodeNameChanged(string name) {
        if (DataNode != null) {
            DataNode.Name = name;
        }
        Title = name;
    }

    public bool RequestSlotConnection(long fromPort, string toNodeName, long toPort) {
        if (Drawer == null) return false;
        return Drawer.RequestSlotConnection(fromPort, toNodeName, toPort);
    }

    public static IEnumerable<Type> GetCreatableNodes() {
        return NodeDrawerMap.Keys.OrderBy(type => type.Name);
    }
}

public record struct SlotMetadata(int Type, Color Color) {
    public static SlotMetadata Node = new(0, Colors.AliceBlue);
}

