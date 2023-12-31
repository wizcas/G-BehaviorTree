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
            slot.DataChild = child;
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
        if (slot.DataChild != null) {
            parent.MoveChild(slot.DataChild, slot.ChildIndex);
        }
        Callable.From(Refresh).CallDeferred();
    }

    private void Refresh() {
        foreach (ChildNodeSlot slot in GraphNode.GetChildren()
                                                .Where(child => child is ChildNodeSlot)
                                                .Cast<ChildNodeSlot>()) {
            slot.UpdateGUI();
        }
        GraphNode.Graph?.RefreshConnections();
    }

    public override bool RequestSlotConnection(long fromPort, string toNodeName, long toPort) {
        if (DataNode == null) {
            return false;
        }

        ISlot? fromSlot = FindSlotByOutPort(fromPort);
        if (fromSlot == null || !fromSlot.IsValid || fromSlot is not ChildNodeSlot thisSlot) {
            return false;
        }

        TreeGraphNode? toNode = GraphNode?.Graph?.FindGraphNode(toNodeName);
        if (toNode == null) {
            // If there's no target node, empty this slot
            // If the slot is already empty, do nothing
            if (thisSlot.DataChild == null) {
                return false;
            }
            DataNode?.RemoveChild(thisSlot.DataChild);
            return true;
        }

        if (toPort != ParentSlotIndex) {
            throw new InvalidOperationException("currenly GBTNode can only connect to a Parent slot");
        }

        if (toNode.DataNode == null) {
            throw new InvalidOperationException("the target node is not a GBTNode");
        }

        if (toNode.DataNode == thisSlot.DataChild) {
            // If the slot is already set to the target node, do nothing
            return false;
        }

        if (toNode.DataNode.Parent == DataNode) {
            // If the parent is unchanged but the child slot is different,
            // we will swtich the occupied slots
            if (GetSlots().FirstOrDefault(slot => slot is ChildNodeSlot cs && cs.DataChild == toNode.DataNode) is ChildNodeSlot otherSlot) {
                otherSlot.DataChild = thisSlot.DataChild;
                DataNode.SwitchChild(thisSlot.ChildIndex, otherSlot.ChildIndex);
            }
        } else {
            // Otherwise, add the target node to the this composite in the given position
            DataNode.MoveChild(toNode.DataNode, thisSlot.ChildIndex);
        }
        thisSlot.DataChild = toNode.DataNode;
        return true;
    }

    private ISlot? FindSlotByOutPort(long port) {
        return GetSlots().FirstOrDefault(slot => slot.OutPortIndex == port);
    }
}
