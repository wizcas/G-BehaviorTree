using GBT.Nodes;
using Godot;
using Godot.Collections;
using System;

public partial class TreeGraphContextMenu : PopupMenu {
    private Dictionary<int, Callable> _menuItemActions { get; } = new();
    private PopupMenu? _menuCreateNode;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        _menuCreateNode = new() { Name = "CreateNodeMenu" };
        AddChild(_menuCreateNode);
        AddSubmenuItem("Create graph node", _menuCreateNode.Name);
        UpdateCreateNodeMenu();

        IdPressed += OnMenuItemPressed;
        _menuCreateNode.IdPressed += OnMenuItemPressed;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) {
    }

    private void OnMenuItemPressed(long id) {
        if (_menuItemActions.TryGetValue((int)id, out Callable action)) {
            action.Call();
        } else {
            GD.Print($"No graph action found for context menu ID {id}");
        }
    }

    public void AddMenuItems(MenuItemAction[] actions) {
        foreach (MenuItemAction action in actions) {
            AddItem(action.Name, action.ID);
            _menuItemActions[action.ID] = Callable.From(action.Action);
        }
    }

    private void UpdateCreateNodeMenu() {
        if (_menuCreateNode == null) return;
        foreach (Type nodeType in TreeGraphNode.GetCreatableNodes()) {
            var id = nodeType.GetHashCode();
            _menuCreateNode.AddItem(nodeType.Name, id);
            _menuItemActions[id] = Callable.From(() => {
                if (Activator.CreateInstance(nodeType) is GBTNode node) {
                    TreeGraph graph = GetNode<TreeGraph>("..");
                    graph.CreateGraphNode(node);
                }
            });
        }
    }
}

public record MenuItemAction(int ID, string Name, Action Action);
