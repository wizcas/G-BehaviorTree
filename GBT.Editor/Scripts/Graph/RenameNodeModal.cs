using Godot;
using System;

public partial class RenameNodeModal : Popup {
    public event Action<TreeGraphNode?, string>? NameChanged;
    #region GUI
    [Export] private LineEdit? _editName;
    [Export] private Button? _buttonOK;
    #endregion

    private TreeGraphNode? _currentNode;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        if (_editName != null) {
            _editName.GuiInput += OnEditNameInput;
        }
        if (_buttonOK != null) {
            _buttonOK.Pressed += OnButtonOKPressed;
        }
    }

    private void OnEditNameInput(InputEvent @event) {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed) {
            if ((keyEvent.Keycode is Key.Enter or Key.KpEnter)) {
                OnButtonOKPressed();
            } else if (keyEvent.Keycode == Key.Escape) {
                Hide();
            }
        }
    }

    private void OnButtonOKPressed() {
        var newName = _editName?.Text ?? string.Empty;
        if (string.IsNullOrEmpty(newName)) {
            // TODO: error message or a hint to show the reason;
            return;
        }
        NameChanged?.Invoke(_currentNode, newName);
        Hide();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) {
    }

    public void Show(TreeGraphNode node) {
        if (_editName == null) return;
        _currentNode = node;
        _editName.Text = node.DataNode?.Name ?? string.Empty;
        Show();
        _editName.GrabFocus();
        _editName.SelectAll();
    }
}
