using GBT.Nodes;
using Godot;
using System;
using System.Diagnostics;
using System.Linq;

public class ListCompositeNodeDrawer : GBTNodeDrawer<ListCompositeNode> {
    public ListCompositeNodeDrawer(TreeGraphNode graphNode) : base(graphNode) {
    }

    private PackedScene _slotScene = GD.Load<PackedScene>("res://Controls/ChildNodeEntry.tscn");

    protected override void OnDrawSlots(ListCompositeNode node, ref int slotIndex) {
        var buttonAddChild = new Button() {
            Name = "ButtonAddChild",
            Text = "Add Child",
        };
        buttonAddChild.Pressed += OnAddChildPressed;
        GraphNode.AddChild(buttonAddChild);
        foreach (GBTNode child in node.Children) {
            AddSlot(child);
            slotIndex++;
        }
        if (IsFirstDraw && !node.Children.Any()) {
            AddSlot(null);
        }
        Callable.From(Refresh).CallDeferred();
    }

    private ChildNodeSlot AddSlot(GBTNode? child) {
        ChildNodeSlot slot = _slotScene.Instantiate<ChildNodeSlot>();
        GraphNode.AddChild(slot);
        // add the slot before the "Add Child" button
        GraphNode.MoveChild(slot, -2);
        slot.DataChild = child;
        if (slot.ButtonMoveUp != null) {
            slot.ButtonMoveUp.Pressed += () => OnMoveButtonPressed(DataNode, slot, -1);
        }
        if (slot.ButtonMoveDown != null) {
            slot.ButtonMoveDown.Pressed += () => OnMoveButtonPressed(DataNode, slot, 1);
        }
        GraphNode.SetSlot(slot.SlotIndex,
            false, SlotMetadata.Node.Type, SlotMetadata.Node.Color,
            true, SlotMetadata.Node.Type, SlotMetadata.Node.Color);
        return slot;
    }

    private void OnAddChildPressed() {
        ChildNodeSlot slot = AddSlot(null);
        Callable.From(slot.UpdateGUI);
    }

    private void OnMoveButtonPressed(ListCompositeNode? parent, ChildNodeSlot slot, int delta) {
        var newIndex = slot.SlotIndex + delta;
        var childSlotCount = GetSlots<ChildNodeSlot>().Count();
        // Limit the movement in the child slot range
        if (newIndex == 0) {
            newIndex = ChildSlotStartIndex + childSlotCount - 1;
        } else if (newIndex > childSlotCount) {
            newIndex = ChildSlotStartIndex;
        }
        GraphNode.MoveChild(slot, newIndex);
        if (parent != null && slot.DataChild != null) {
            parent.MoveChild(slot.DataChild, slot.GetDesignedChildIndex());
        }
        Callable.From(Refresh).CallDeferred();
    }

    private void Refresh() {
        foreach (ChildNodeSlot slot in GetSlots<ChildNodeSlot>()) {
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

        // Remove the connection from the requesting slot to its existing target
        GBTNode? oldTargetDataNode = GraphNode?.Graph?.FindGraphNode(thisSlot.TargetNodeName)?.DataNode;
        if (oldTargetDataNode != null) {
            oldTargetDataNode.Parent = null;
        }

        TreeGraphNode? toNode = GraphNode?.Graph?.FindGraphNode(toNodeName);
        if (toNode == null
            || toPort < 0
            || toNode.GetSlotTypeLeft(toNode.Drawer?.FindSlotByInPort(toPort)?.SlotIndex ?? -1) != SlotMetadata.Node.Type) {
            // If the target node or port is invalid, empty this slot
            // If the slot is already empty, do nothing
            if (thisSlot.DataChild == null) {
                return false;
            }
            // Otherwise, it's an invalid connection which should be removed
            DataNode?.RemoveChild(thisSlot.DataChild);
            thisSlot.DataChild = null;
            PrintDebugInfo();
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

        var thisChildIndex = thisSlot.GetDataChildIndex();
        var needMoveDataChild = true;

        // So far it is guaranteed that there is a GBTNode to attach
        // AND it is different from the one bound to this slot, if any
        if (toNode.DataNode.Parent == DataNode) {
            // If the parent is unchanged but the child slot is different,
            // we will try to swtich the occupied slots
            if (GetSlots().FirstOrDefault(slot => slot is ChildNodeSlot cs && cs.DataChild == toNode.DataNode) is ChildNodeSlot otherSlot) {
                if (thisChildIndex > -1) {
                    // Switch only if both slots have attached GBTNodes
                    // If they are switched, we don't need further node-moving
                    DataNode.SwitchChild(thisChildIndex, otherSlot.GetDataChildIndex());
                    needMoveDataChild = false;
                }
                otherSlot.DataChild = thisSlot.DataChild;
            }
        } else {
            if (toNode.DataNode.Parent != null) {
                // Remove the slot connection from the target's old parent to the target
                TreeGraphNode? oldParentOfTarget = GraphNode?.Graph?.FindGraphNode(toNode.DataNode.Parent.ID);
                Callable.From(() => oldParentOfTarget?.Drawer?.RefreshSlots()).CallDeferred();
            }
        }
        if (needMoveDataChild) {
            // Otherwise, move and add (if necessary) the target node to the given position
            DataNode.MoveChild(toNode.DataNode, thisSlot.GetDesignedChildIndex());
        }
        thisSlot.DataChild = toNode.DataNode;
        PrintDebugInfo();
        return true;
    }

    public override void RefreshSlots() {
        base.RefreshSlots();
        if (DataNode != null) {
            foreach (ChildNodeSlot childSlot in GetSlots<ChildNodeSlot>()) {
                if (childSlot.DataChild?.Parent != DataNode) {
                    childSlot.DataChild = null;
                }
            }
        }
    }

    private void PrintDebugInfo() {
        var dbg = string.Join(",", DataNode?.Children.Select(child => child.Name) ?? []);
        GD.Print(dbg);
        Debug.WriteLine(dbg);
    }
}
