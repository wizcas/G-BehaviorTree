using GBT.Nodes;
using Godot;

[GlobalClass]
public partial class ChildNodeEntry : Control {
    #region GUI Nodes
    [Export] private Label? LabelNodeIndex { get; set; }
    [Export] private Label? LabelChildName { get; set; }
    [Export] private Button? ButtonMoveUp { get; set; }
    [Export] private Button? ButtonMoveDown { get; set; }
    #endregion

    #region Properties

    private int _index;
    private GBTNode? child;

    public int Index {
        get => _index;
        set {
            _index = value;
            if (LabelNodeIndex != null) {
                LabelNodeIndex.Text = $"[{_index}]";
            }
        }
    }
    public GBTNode? Child {
        get => child;
        set {
            child = value;
            if (LabelChildName != null) {
                if (child == null) {
                    LabelChildName.Text = "(Empty)";
                } else {
                    LabelChildName.Text = child.Name;
                }
            }
        }
    }

    #endregion


    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) {
    }
}
