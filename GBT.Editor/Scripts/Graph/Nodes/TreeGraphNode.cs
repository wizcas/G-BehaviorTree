using GBT.Nodes;
using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class TreeGraphNode : GraphNode {
    public TreeGraph? Graph { get; private set; }

    private GBTNode? _node;
    public GBTNode? Node {
        get => _node;
        set {
            if (_node == value) return;
            GBTNode? oldNode = _node;
            _node = value;
            UpdateNode(oldNode?.GetType() == _node?.GetType());
        }
    }
    public GBTNodeDrawer? Drawer { get; private set; }

    private static Dictionary<Type, Func<TreeGraphNode, GBTNodeDrawer>> NodeDrawerMap = new() {
        { typeof(SequenceNode), (graphNode)=>new ListCompositeNodeDrawer(graphNode) },
    };

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        Graph = GetNode<TreeGraph>("..");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) {
    }

    private void UpdateNode(bool shouldUpdateDrawer) {
        ClearAllSlots();

        Name = Node?.ID ?? "EmptyGraphNode";
        Title = Node?.Name ?? "(No Node)";

        if (Node == null) return;
        if (Drawer == null || shouldUpdateDrawer) {
            // Create graph slot drawer by GBT Node types
            if (NodeDrawerMap.TryGetValue(Node.GetType(), out Func<TreeGraphNode, GBTNodeDrawer>? drawerFactory)) {
                Drawer = drawerFactory(this);
            } else {
                Drawer = new UnknownNodeDrawer(this);
            }
        }
        Drawer.DrawSlots(Node);
    }
}

public record struct SlotMetadata(int Type, Color Color) {
    public static SlotMetadata Node = new(0, Colors.AliceBlue);
}

