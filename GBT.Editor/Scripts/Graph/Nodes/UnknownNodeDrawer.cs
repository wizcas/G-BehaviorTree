using GBT.Nodes;
using Godot;

public class UnknownNodeDrawer : GBTNodeDrawer {
    public UnknownNodeDrawer(TreeGraphNode graphNode) : base(graphNode) { }
    public override void DrawSlots(GBTNode node) {
        base.DrawSlots(node);
        GraphNode.AddChild(new Label() {
            Name = "UnsupportedNodeType",
            Text = "Unsupported node type",
        });
    }
}
