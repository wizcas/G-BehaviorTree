using Godot;
using System.Collections.Generic;

public class ShortcutManager {
    public static ShortcutManager Instance { get; } = new ShortcutManager();

    private Dictionary<string, Shortcut> _map = new();

    public Shortcut? Get(string name) {
        if (!_map.TryGetValue(name, out Shortcut? shortcut)) {
            shortcut = ResourceLoader.Load<Shortcut>($"res://Shortcuts/{name}.tres");
            if (shortcut != null) {
                _map[name] = shortcut;
            }
        }
        if (shortcut == null) {
            GD.PrintErr($"Shortcut {name} not found");
        }
        return shortcut;
    }
}
