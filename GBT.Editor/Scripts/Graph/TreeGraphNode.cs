using GBT.Nodes;
using Godot;

[GlobalClass]
public partial class TreeGraphNode : GraphNode {
    public enum SlotType {
        Node,
    }

    private GBTNode? _node;
    public GBTNode? Node {
        get => _node;
        set {
            if (_node == value) return;
            _node = value;
            UpdateNode();
        }
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) {
    }

    private void UpdateNode() {
        ClearAllSlots();
        Name = Node?.ID ?? "EmptyGraphNode";
        Title = Node?.Name ?? "(No Node)";
        AddChild(new Label() { Name = "Parent", Text = "Parent" });
        SetSlot(0,
            true, SlotMetadata.Node.Type, SlotMetadata.Node.Color,
            false, SlotMetadata.Node.Type, SlotMetadata.Node.Color);
        var slotIndex = 1;
        switch (Node) {
            case ListCompositeNode listComposite:
                foreach (GBTNode child in listComposite.Children) {
                    AddChild(new Label() {
                        Name = $"Slot {slotIndex}",
                        Text = child.Name,
                    });
                    SetSlot(slotIndex,
                        false, SlotMetadata.Node.Type, SlotMetadata.Node.Color,
                        true, SlotMetadata.Node.Type, SlotMetadata.Node.Color);
                    slotIndex++;
                }
                break;
            default:
                AddChild(new Label() { Text = "Unsupported node type" });
                break;
        }
    }
}

public record struct SlotMetadata(int Type, Color Color) {
    public static SlotMetadata Node = new(0, Colors.AliceBlue);
}

