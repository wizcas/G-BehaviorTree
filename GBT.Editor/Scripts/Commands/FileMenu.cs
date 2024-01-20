using Godot;
using System;
using System.Collections.Generic;

public partial class FileMenu : PopupMenu {
    private enum ItemIndex {
        New = 0,
        Open,
        Save,
        SaveAs,
        // == Separator== 
        Quit = 5
    }

    [Export] private Shortcut? Shortcut { get; set; }

    private Dictionary<ItemIndex, Action> _menuActionMap = new() {
        { ItemIndex.New, () => GraphManager.GetInstance().New() },
        { ItemIndex.Open, () => GraphManager.GetInstance().Open() },
        { ItemIndex.Save, () => GraphManager.GetInstance().Save() },
        { ItemIndex.SaveAs, () => GraphManager.GetInstance().SaveAs() },
        { ItemIndex.Quit, () => (Engine.GetMainLoop() as SceneTree)?.Quit() },
    };

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        RegisterShortcuts();
        IndexPressed += OnIndexPressed;
    }

    private void RegisterShortcuts() {
        foreach (ItemIndex itemEnum in Enum.GetValues<ItemIndex>()) {
            var index = ((int)itemEnum);
            var name = Enum.GetName(itemEnum);
            if (name == null) continue;
            Shortcut? shortcut = ShortcutManager.Instance.Get(name);
            if (shortcut != null) {
                SetItemShortcut(index, shortcut, true);
            }
        }
    }

    private void OnIndexPressed(long index) {
        if (_menuActionMap.TryGetValue((ItemIndex)index, out Action? action)) {
            action.Invoke();
        }
    }

    private void Quit() {
        GetTree().Quit();
    }
}
