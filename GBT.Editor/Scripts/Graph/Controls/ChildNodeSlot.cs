using GBT.Nodes;
using Godot;

[GlobalClass]
public partial class ChildNodeSlot : Control {
    #region Node GUI
    [Export] public Label? LabelNodeIndex { get; set; }
    [Export] public Label? LabelChildName { get; set; }
    [Export] public Button? ButtonMoveUp { get; set; }
    [Export] public Button? ButtonMoveDown { get; set; }

    public TreeGraphNode GraphNode => GetParent<TreeGraphNode>();
    #endregion

    #region Properties

    private int _index;
    private GBTNode? _child;

    public int SlotIndex => GetIndex();
    public int ChildIndex => SlotIndex - 1;
    public GBTNode? Child {
        get => _child;
        set {
            _child = value;
            UpdateGUI();
        }
    }

    #endregion

    public void UpdateGUI() {
        if (LabelNodeIndex != null) {
            LabelNodeIndex.Text = $"[{ChildIndex}]";
        }
        if (LabelChildName != null) {
            if (_child == null) {
                LabelChildName.Text = "(Empty)";
            } else {
                LabelChildName.Text = _child.Name;
            }
        }
    }
    public void UpdateGUIDeferred() {
        Callable.From(UpdateGUI).CallDeferred();
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) {
    }
}
