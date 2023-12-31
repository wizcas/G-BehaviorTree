using GBT.Nodes;
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
    public const int SlotStartIndex = 1;
    public TreeGraphNode GraphNode { get; } = graphNode;
    public GBTNode? DataNode => GraphNode.DataNode;

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

    public IEnumerable<ISlot> GetSlots() {
        return GraphNode.GetChildren().Where(child => child is ISlot slot && slot.IsValid).Cast<ISlot>();
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
}

public abstract class GBTNodeDrawer<TDataNode> : GBTNodeDrawer where TDataNode : GBTNode {
    public new TDataNode? DataNode => GraphNode.DataNode as TDataNode;
    public GBTNodeDrawer(TreeGraphNode graphNode) : base(graphNode) { }

    public sealed override void DrawSlots(GBTNode node) {
        base.DrawSlots(node);
        if (node is not TDataNode typedNode) {
            GraphNode.AddChild(new Label() {
                Name = "WrongGBTNodeType",
                Text = $"[REQUIRE {typeof(TDataNode).Name}]",
            });
        } else {
            var slotIndex = SlotStartIndex;
            DrawSlots(typedNode, ref slotIndex);
        }
    }
    public abstract void DrawSlots(TDataNode node, ref int slotIndex);
}
