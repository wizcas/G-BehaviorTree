using GBT.Nodes;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        GetTitlebarHBox().GuiInput += async (e) => {
            if (e is InputEventMouseButton mouseEvent && mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.DoubleClick) {
                // Wait for the mouse event to finish, otherwise it'll drag the node after modal closed
                await Task.Delay(200);
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

    private void UpdateNode(bool forceUpdateDrawer) {
        ClearAllSlots();
        foreach (Node? child in GetChildren()) {
            child.Free();
        }

        Name = DataNode?.ID ?? "EmptyGraphNode";
        Title = DataNode?.Name ?? "(No Node)";

        if (DataNode == null) return;
        if (Drawer == null || forceUpdateDrawer) {
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
        Graph?.RenameNodeModal?.Show(this);
    }

    private void OnNodeNameChanged(TreeGraphNode? sender, string name) {
        if (sender == this) {
            if (DataNode != null) {
                DataNode.Name = name;
            }
            Title = name;
        } else if (DataNode is IParentNode p) {
            // rerender slots in case they should reflect the name change of the child nodes
            if (p.Children.Any(child => sender != null && child == sender?.DataNode)) {
                UpdateNode(false);
            }
        }
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

