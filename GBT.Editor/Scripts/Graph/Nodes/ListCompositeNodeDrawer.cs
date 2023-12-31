using GBT.Nodes;
using Godot;
using System;
using System.Linq;
public class ListCompositeNodeDrawer : GBTNodeDrawer<ListCompositeNode> {
    public ListCompositeNodeDrawer(TreeGraphNode graphNode) : base(graphNode) { }

    public override void DrawSlots(ListCompositeNode node, ref int slotIndex) {
        PackedScene scene = GD.Load<PackedScene>("res://Controls/ChildNodeEntry.tscn");
        foreach (GBTNode child in node.Children) {
            ChildNodeSlot slot = scene.Instantiate<ChildNodeSlot>();
            GraphNode.AddChild(slot);
            GraphNode.SetSlot(slotIndex,
                false, SlotMetadata.Node.Type, SlotMetadata.Node.Color,
                true, SlotMetadata.Node.Type, SlotMetadata.Node.Color);
            slot.Child = child;
            if (slot.ButtonMoveUp != null) {
                slot.ButtonMoveUp.Pressed += () => OnMoveButtonPressed(node, slot, -1);
            }
            if (slot.ButtonMoveDown != null) {
                slot.ButtonMoveDown.Pressed += () => OnMoveButtonPressed(node, slot, 1);
            }
            slotIndex++;
        }
        Callable.From(Refresh).CallDeferred();
    }

    private void OnMoveButtonPressed(ListCompositeNode parent, ChildNodeSlot slot, int delta) {
        var newIndex = slot.SlotIndex + delta;
        // Always preserve child node index 0 for the Parent slot
        if (newIndex == 0) {
            newIndex = -1;
        } else if (newIndex > parent.Children.Count()) {
            newIndex = 1;
        }
        GraphNode.MoveChild(slot, newIndex);
        if (slot.Child != null) {
            parent.MoveChild(slot.Child, slot.ChildIndex);
        }
        Callable.From(Refresh).CallDeferred();
    }

    private void Refresh() {
        foreach (ChildNodeSlot slot in GraphNode.GetChildren()
                                                .Where(child => child is ChildNodeSlot)
                                                .Cast<ChildNodeSlot>()) {
            slot.UpdateGUI();
        }
        GraphNode.Graph?.ReconnectNodes();
    }
}
