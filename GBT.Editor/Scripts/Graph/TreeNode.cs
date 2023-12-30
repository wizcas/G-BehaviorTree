using GBT.Sharp.Core.Nodes;
using Godot;

[GlobalClass]
public partial class TreeNode : GraphNode {
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
        Title = Node?.Name ?? "(No Node)";
        AddChild(new Label() { Name = "Parent", Text = "Parent" });
        SetSlot(0, true, 0, Colors.AliceBlue, false, 0, Colors.Transparent);
        var slotIndex = 1;
        if (Node is ListCompositeNode listComposite) {
            foreach (GBTNode child in listComposite.Children) {
                AddChild(new Label() {
                    Name = $"Slot {slotIndex}",
                    Text = child.Name,
                });
                slotIndex++;
            }
        }
    }
}

