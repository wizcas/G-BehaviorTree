using GBT.Nodes;
using Godot;
using System;
using System.Linq;

[GlobalClass]
public partial class ChildNodeSlot : Control, ISlot {
    #region Node GUI
    [Export] public Label? LabelNodeIndex { get; set; }
    [Export] public Label? LabelChildName { get; set; }
    [Export] public Button? ButtonMoveUp { get; set; }
    [Export] public Button? ButtonMoveDown { get; set; }
    [Export] public Button? ButtonDelete { get; set; }

    public TreeGraphNode GraphNode => GetParent<TreeGraphNode>();
    #endregion

    #region Properties

    private int _index;
    private GBTNode? _child;

    public string OwnerNodeName => GraphNode.Name;
    public string TargetNodeName => DataChild?.ID ?? string.Empty;
    public int SlotIndex => GetIndex();
    public int InPortIndex => -1;
    public int OutPortIndex => SlotIndex - GBTNodeDrawer.ChildSlotStartIndex;
    public bool IsValid => true;

    public GBTNode? DataChild {
        get => _child;
        set {
            _child = value;
            UpdateGUI();
        }
    }

    #endregion

    public int GetDataChildIndex() {
        if (DataChild == null) {
            return -1;
        }
        return (DataChild.Parent as IParentNode)?.GetChildIndex(DataChild) ?? -1;
    }
    public int GetDesignedChildIndex() {
        if (GraphNode == null) {
            throw new InvalidOperationException($"the slot doesn't have a TreeNodeGraph parent");
        }
        if (GraphNode.Drawer == null) {
            throw new InvalidOperationException($"the parent TreeNodeGraph doesn't have a valid GBTNodeDrawer");
        }
        //return GraphNode.Drawer.GetSlots<ChildNodeSlot>().Where(slot => slot.DataChild != null).ToList().IndexOf(this);
        var slots = GraphNode.Drawer.GetSlots<ChildNodeSlot>().ToList();
        var index = 0;
        for (var i = 0; i < slots.Count; i++) {
            ChildNodeSlot slot = slots[i];
            if (slot == this) {
                break;
            }
            if (slot.DataChild != null) {
                index++;
            }
        }
        return index;
    }

    public void UpdateGUI() {
        if (LabelNodeIndex != null) {
            LabelNodeIndex.Text = $"[{OutPortIndex}]";
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

    public override string ToString() {
        return $"[Slot #{SlotIndex}, Child #{OutPortIndex}] {DataChild?.Name ?? "(null)"}";

    }
}
