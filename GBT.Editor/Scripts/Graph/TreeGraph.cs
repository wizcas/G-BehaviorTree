using Godot;
using Godot.Collections;
using System;

public partial class TreeGraph : GraphEdit {
    private PopupMenu _contextMenu;
    private Dictionary<int, Callable> _contextActions;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        InitializeContextMenu();
        GuiInput += OnGuiInput;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) {
    }

    private void OnGuiInput(InputEvent @event) {
        if (@event is InputEventMouseButton mouseButton) {
            GD.Print("mouse button:", mouseButton.ButtonIndex, mouseButton.Pressed);
            if (mouseButton.ButtonIndex == MouseButton.Right && mouseButton.Pressed) {
                Vector2 mousePos = GetLocalMousePosition();
                _contextMenu.Position = new Vector2I((int)mousePos.X, (int)mousePos.Y);
                _contextMenu.Show();
            }
        }
    }

    private void InitializeContextMenu() {
        _contextMenu = GetNode<PopupMenu>("ContextMenu");
        _contextMenu.AddItem("Create Test Node", 0);
        _contextMenu.IdPressed += OnContextMenuIDPressed;
        _contextActions = new(){
            {0, Callable.From(CreateTestNode)},
        };

    }

    private void OnContextMenuIDPressed(long id) {
        if (_contextActions.TryGetValue((int)id, out Callable action)) {
            action.Call();
        } else {
            GD.Print($"No graph action found for context menu ID {id}");
        }
    }

    private void CreateTestNode() {
        var node = new GraphNode() {
            Name = "TestNode",
            Title = $"Test Node ({DateTime.Now.ToString("yyyyMMdd-HHmmss")})",
            PositionOffset = (GetViewport().GetMousePosition() + ScrollOffset) / Zoom,
        };
        AddChild(node);
    }
}
