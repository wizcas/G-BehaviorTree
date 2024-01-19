using GBT.Nodes;
using Godot;

public class UnknownNodeDrawer : GBTNodeDrawer {
    public UnknownNodeDrawer(TreeGraphNode graphNode) : base(graphNode) { }
    protected override void OnDrawSlots(GBTNode node) {
        base.OnDrawSlots(node);
        GraphNode.AddChild(new Label() {
            Name = "UnsupportedNodeType",
            Text = "Unsupported node type",
        });
    }

    public override bool RequestSlotConnection(long fromPort, string toNodeName, long toPort) {
        return false;
    }
}
