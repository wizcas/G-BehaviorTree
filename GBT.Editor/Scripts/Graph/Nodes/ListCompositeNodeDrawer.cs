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
            Icon = ResourceLoader.Load<Texture2D>("res://Icons/UI/plus.svg"),
        };
        buttonAddChild.Pressed += OnAddChildPressed;
        var margin = new MarginContainer();
        margin.AddThemeConstantOverride("margin_top", 16);
        margin.AddChild(buttonAddChild);
        GraphNode.AddChild(margin);
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
        if (slot.ButtonDelete != null) {
            slot.ButtonDelete.Pressed += () => OnDeleteButtonPressed(DataNode, slot);
        }
        GraphNode.SetSlot(slot.SlotIndex,
            false, SlotMetadata.Node.Type, SlotMetadata.Node.Color,
            true, SlotMetadata.Node.Type, SlotMetadata.Node.Color);
        return slot;
    }

    private void OnAddChildPressed() {
        ChildNodeSlot slot = AddSlot(null);
        slot.UpdateGUI();
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
    private void OnDeleteButtonPressed(ListCompositeNode? parent, ChildNodeSlot slot) {
        if (parent != null && slot.DataChild != null) {
            parent.RemoveChild(slot.DataChild);
        }
        GraphNode.RemoveChild(slot);
        slot.QueueFree();
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

        GBTNode? prevTargetNode = GraphNode?.Graph?.FindGraphNode(thisSlot.TargetNodeName)?.DataNode;

        TreeGraphNode? toNode = GraphNode?.Graph?.FindGraphNode(toNodeName);
        if (toNode == null
            || toPort < 0
            || toNode.GetSlotTypeLeft(toNode.Drawer?.FindSlotByInPort(toPort)?.SlotIndex ?? -1) != SlotMetadata.Node.Type) {
            // When the target node or port is invalid
            if (thisSlot.DataChild == null) {
                // If the slot is already empty, do nothing
                return false;
            }
            // Otherwise, it's an invalid connection which should be removed
            DataNode?.RemoveChild(thisSlot.DataChild);
            thisSlot.DataChild = null;
        } else {
            // When there is indeed a target node to connect to
            if (toPort != ParentSlotIndex) {
                throw new InvalidOperationException("currenly GBTNode can only connect to a Parent slot");
            }

            if (toNode.DataNode == null) {
                throw new InvalidOperationException("the target node is not a GBTNode");
            }

            if (toNode.DataNode == thisSlot.DataChild) {
                // If the connection is not changed, do nothing
                return false;
            }

            var currentChildDataIndex = thisSlot.GetDataChildIndex();
            var willMoveDataChild = true;

            // So far it is guaranteed that there is a GBTNode to attach
            // AND it is different from the one bound to this slot, if any
            if (toNode.DataNode.Parent == DataNode) {
                // If the parent is unchanged but the child slot is reassigned,
                // we will try to swtich the occupied slots
                if (GetSlots().FirstOrDefault(slot => slot is ChildNodeSlot cs && cs.DataChild == toNode.DataNode) is ChildNodeSlot prevAssignedSlot) {
                    if (currentChildDataIndex > -1) {
                        // Switch the actual children data only if both slots have attached GBTNodes
                        // If they are switched, we don't need further node-moving below
                        DataNode.SwitchChild(currentChildDataIndex, prevAssignedSlot.GetDataChildIndex());
                        willMoveDataChild = false;
                    }
                    // Old slot's binding data needs to be updated
                    // (this slot's binding data will be updated below once for all)
                    prevAssignedSlot.DataChild = thisSlot.DataChild;
                }
            } else {
                if (toNode.DataNode.Parent != null) {
                    // Update the graph connection of the target's old parent in the next frame
                    // after we've reassigned target's parent in this frame (by call MoveChild)
                    TreeGraphNode? oldParentOfTarget = GraphNode?.Graph?.FindGraphNode(toNode.DataNode.Parent.ID);
                    Callable.From(() => oldParentOfTarget?.Drawer?.RefreshSlots()).CallDeferred();
                }
            }
            if (willMoveDataChild) {
                // Otherwise, move and add (if necessary) the target node to the given position
                DataNode.MoveChild(toNode.DataNode, thisSlot.GetDesignedChildIndex());
            }
            thisSlot.DataChild = toNode.DataNode;

            // check if the slot's old target child is now an orphan
            if (prevTargetNode != null && !GetSlots().Any(slot => slot is ChildNodeSlot cs && cs.DataChild == prevTargetNode)) {
                prevTargetNode.Parent = null;
            }
        }
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
