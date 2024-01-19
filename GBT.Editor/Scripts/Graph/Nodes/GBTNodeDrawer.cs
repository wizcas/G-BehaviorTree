﻿using GBT.Nodes;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public record struct SlotConnection(string FromNode, int FromPort, string ToNode, int ToPort) {
    public readonly static SlotConnection Empty = new("", -1, "", -1);
}
public interface ISlot {
    public string OwnerNodeName { get; }
    public string TargetNodeName { get; }
    public int SlotIndex { get; }
    public int InPortIndex { get; }
    public int OutPortIndex { get; }
    public bool IsValid { get; }
}

public abstract class GBTNodeDrawer(TreeGraphNode graphNode) {
    public const int ParentSlotIndex = 0;
    public const int ChildSlotStartIndex = 1;
    public TreeGraphNode GraphNode { get; } = graphNode;
    public GBTNode? DataNode => GraphNode.DataNode;
    protected bool IsFirstDraw = true;

    public void DrawSlots(GBTNode node) {
        BeforeDrawSlots(node);
        if (GraphNode == null) {
            throw new InvalidOperationException("This drawer is not attached to a GraphNode");
        }
        // Common parent slot
        GraphNode.AddChild(new Label() { Name = "Parent", Text = "Parent" });
        GraphNode.SetSlot(ParentSlotIndex,
            true, SlotMetadata.Node.Type, SlotMetadata.Node.Color,
            false, SlotMetadata.Node.Type, SlotMetadata.Node.Color);
        OnDrawSlots(node);
        AfterDrawSlots(node);
        IsFirstDraw = false;
    }
    protected virtual void BeforeDrawSlots(GBTNode node) { }
    protected virtual void AfterDrawSlots(GBTNode node) { }
    protected virtual void OnDrawSlots(GBTNode node) { }

    public IEnumerable<ISlot> GetSlots() {
        return GraphNode.GetChildren().Where(child => child is ISlot slot && slot.IsValid).Cast<ISlot>();
    }
    public IEnumerable<TSlot> GetSlots<TSlot>() where TSlot : ISlot {
        return GetSlots().Where(slot => slot is TSlot).Cast<TSlot>();
    }

    public virtual IEnumerable<SlotConnection> GetSlotConnections() {
        return GetSlots().Select(slot => new SlotConnection(GraphNode.Name, slot.OutPortIndex, slot.TargetNodeName, ParentSlotIndex));
    }
    public abstract bool RequestSlotConnection(long fromPort, string toNodeName, long toPort);

    public ISlot? FindSlotByInPort(long port) {
        return GetSlots().FirstOrDefault(slot => slot.InPortIndex == port);
    }
    public ISlot? FindSlotByOutPort(long port) {
        return GetSlots().FirstOrDefault(slot => slot.OutPortIndex == port);
    }
    public virtual void RefreshSlots() { }
}

public abstract class GBTNodeDrawer<TDataNode> : GBTNodeDrawer where TDataNode : GBTNode {
    public new TDataNode? DataNode => GraphNode.DataNode as TDataNode;
    public GBTNodeDrawer(TreeGraphNode graphNode) : base(graphNode) { }

    protected sealed override void OnDrawSlots(GBTNode node) {
        base.OnDrawSlots(node);
        if (node is not TDataNode typedNode) {
            GraphNode.AddChild(new Label() {
                Name = "WrongGBTNodeType",
                Text = $"[REQUIRE {typeof(TDataNode).Name}]",
            });
        } else {
            var slotIndex = ChildSlotStartIndex;
            OnDrawSlots(typedNode, ref slotIndex);
        }
    }
    protected abstract void OnDrawSlots(TDataNode node, ref int slotIndex);
}
