using GBT.Nodes;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public record struct SlotConnection(string FromNode, int FromPort, string ToNode, int ToPort) {
    public readonly static SlotConnection Empty = new("", -1, "", -1);
}

public abstract class GBTNodeDrawer(TreeGraphNode graphNode) {
    public const int ParentSlotIndex = 0;
    public const int SlotStartIndex = 1;
    public TreeGraphNode GraphNode { get; } = graphNode;

    public virtual void DrawSlots(GBTNode node) {
        if (GraphNode == null) {
            throw new InvalidOperationException("This drawer is not attached to a GraphNode");
        }
        // Common parent slot
        GraphNode.AddChild(new Label() { Name = "Parent", Text = "Parent" });
        GraphNode.SetSlot(ParentSlotIndex,
            true, SlotMetadata.Node.Type, SlotMetadata.Node.Color,
            false, SlotMetadata.Node.Type, SlotMetadata.Node.Color);

    }

    public virtual IEnumerable<SlotConnection> GetSlotConnections() {
        return GraphNode.GetChildren().Where(child => child is ChildNodeSlot slot && slot.Child != null).Cast<ChildNodeSlot>()
            .Select(slot => new SlotConnection(GraphNode.Name, slot.ChildIndex, slot.Child!.ID, ParentSlotIndex));
    }
}

public abstract class GBTNodeDrawer<TNode> : GBTNodeDrawer where TNode : GBTNode {
    public GBTNodeDrawer(TreeGraphNode graphNode) : base(graphNode) { }

    public sealed override void DrawSlots(GBTNode node) {
        base.DrawSlots(node);
        if (node is not TNode typedNode) {
            GraphNode.AddChild(new Label() {
                Name = "WrongGBTNodeType",
                Text = $"[REQUIRE {typeof(TNode).Name}]",
            });
        } else {
            var slotIndex = SlotStartIndex;
            DrawSlots(typedNode, ref slotIndex);
        }
    }
    public abstract void DrawSlots(TNode node, ref int slotIndex);
}
