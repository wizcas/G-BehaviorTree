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

    private Control? _rootBadge;

    private static Dictionary<Type, Func<TreeGraphNode, GBTNodeDrawer>> NodeDrawerMap = new() {
        { typeof(SequenceNode), (graphNode)=>new ListCompositeNodeDrawer(graphNode) },
        { typeof(SelectorNode), (graphNode)=>new ListCompositeNodeDrawer(graphNode) },
    };

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        Graph = GetNode<TreeGraph>("..");
        Graph.RootChanged += UpdateRootState;
        Resizable = true;
        HBoxContainer titlebar = GetTitlebarHBox();
        titlebar.GuiInput += OnTitlebarGuiInput;
        _rootBadge = new Label() { Text = "Root", Visible = false };
        _rootBadge.AddThemeColorOverride("font_color", Colors.Yellow);
        titlebar.AddChild(_rootBadge);
        var renameButton = new Button() { Text = "Re" }; // TODO: icon
        renameButton.Pressed += ShowRenameModal;
        titlebar.AddChild(renameButton);
        if (Graph.RenameNodeModal != null) {
            Graph.RenameNodeModal.NameChanged += OnNodeNameChanged;
        }
    }
    private async void OnTitlebarGuiInput(InputEvent e) {
        if (e is InputEventMouseButton mouseEvent && mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.DoubleClick) {
            // Wait for the mouse event to finish, otherwise it'll drag the node after modal closed
            await Task.Delay(200);
            ShowRenameModal();
        }
    }
    private void OnNodeNameChanged(TreeGraphNode? sender, string name) {
        var graphDataUpdated = false;
        if (sender == this) {
            if (DataNode != null) {
                DataNode.Name = name;
            }
            Title = name;
        } else if (DataNode is IParentNode parent) {
            // rerender slots in case they should reflect the name change of the child nodes
            if (parent.Children.Any(child => sender != null && child == sender?.DataNode)) {
                UpdateNode(false);
                graphDataUpdated = true;
            }
        }
        if (!graphDataUpdated) {
            Graph?.UpdateJsonOutput();
        }
    }
    public void Delete() {
        Graph?.RemoveChild(this);
        QueueFree();
        if (DataNode != null) {
            if (DataNode.Parent != null) {
                TreeGraphNode? parentNode = Graph?.FindGraphNode(DataNode.Parent.ID);
                DataNode.Parent = null;
                // Update the old parent's slots for the deleted child
                parentNode?.Drawer?.RefreshSlots();
            }
        }
        if (DataNode is IParentNode selfAsParent) {
            GBTNode[] children = selfAsParent.Children.ToArray();
            foreach (GBTNode child in children) {
                child.Parent = null;
            }
        }
        Callable.From(() => UpdateNode(false)).CallDeferred();
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
        Graph?.RefreshConnections();
    }

    private void ShowRenameModal() {
        Graph?.RenameNodeModal?.Show(this);
    }

    public bool RequestSlotConnection(long fromPort, string toNodeName, long toPort) {
        if (Drawer == null) return false;
        return Drawer.RequestSlotConnection(fromPort, toNodeName, toPort);
    }

    public static IEnumerable<Type> GetCreatableNodes() {
        return NodeDrawerMap.Keys.OrderBy(type => type.Name);
    }

    private void UpdateRootState() {
        if (_rootBadge != null) {
            _rootBadge.Visible = Graph?.RootNode == DataNode;
        }
    }
}

public record struct SlotMetadata(int Type, Color Color) {
    public static SlotMetadata Node = new(0, Colors.AliceBlue);
}

