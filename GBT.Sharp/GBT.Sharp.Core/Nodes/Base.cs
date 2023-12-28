using MessagePack;
using NanoidDotNet;

namespace GBT.Sharp.Core.Nodes;

/// <summary>
/// The base class of all Behavior Tree Nodes, which defines
/// the basic properties, methods, and behaviors of a node.
/// </summary>
public abstract class Node {
    public string ID { get; }
    public string Name { get; set; }

    private NodeState _state;
    public NodeState State {
        get => _state;
        set {
            if (_state != value) {
                Context?.Trace.Add(this, $"state change: {_state} -> {value}");
            }
            _state = value;
        }
    }

    private Node? _parent;
    public Node? Parent { get => _parent; set => SetParent(value); }

    public bool IsDisabled { get; set; }

    private TreeContext? _context;
    public TreeContext? Context {
        get => _context;
        set {
            if (_context != value) {
                _context = value;
                OnContextChanged();
            }
        }
    }
    private NodeContext? _nodeContext;
    public NodeContext NodeContext => _nodeContext ??= Context?.GetNodeContext<NodeContext>(this)!;

    public Node(string id, string name) {
        ID = id;
        Name = name;
        Reset();
    }
    public Node(string name) : this(Nanoid.Generate(), name) { }
    public Node() : this("") {
        Name = $"New {GetType().Name}";
    }

    /// <summary>
    /// Set the node ready for running.
    /// This method is for internal purpose.
    /// </summary>
    public virtual void Initialize() {
        Context?.Trace.Add(this, "initialize");
    }

    /// <summary>
    /// Called everytime the node is executed.
    /// </summary>
    public void Tick() {
        if (Context is null) {
            BehaviorTree.Logger.Error("this node has no context", this);
            return;
        }
        if (IsDisabled) {
            Context.Trace.Add(this, "skip: disabled");
            return;
        }
        if (State != NodeState.Running) {
            Initialize();
            Context.EnterNode(this);
        }
        State = NodeState.Running;
        Context.Trace.Add(this, "tick");
        DoTick();
        TryExit();
    }
    protected abstract void DoTick();

    /// <summary>
    /// Check whether this node has done execution and can be exited. 
    /// If yes, any exit steps of the node will be run.
    /// </summary>
    protected void TryExit() {
        if (State != NodeState.Running) {
            CleanUp();

            if (State != NodeState.Unvisited) {
                Context?.ExitNode(this);
            }
        }
    }
    /// <summary>
    /// Clean up any intermediate state or data that was set
    /// by running this node.
    /// </summary>
    public virtual void CleanUp() {
        Context?.Trace.Add(this, "clean up");
    }
    /// <summary>
    /// Reset this node to its initial state and data.
    /// </summary>
    public virtual void Reset() {
        Context?.Trace.Add(this, "reset");
        if (State != NodeState.Unvisited) {
            CleanUp();
        }
        State = NodeState.Unvisited;
    }
    protected virtual void OnContextChanged() {
        _nodeContext = null;
    }

    private void SetParent(Node? parent) {
        if (parent == Parent) {
            return;
        }

        if (parent is not null and not IParentNode) {
            throw new ArgumentException("only IParentNode can be set as parent", nameof(parent));
        }

        if (Parent is not null and IParentNode oldParent) {
            oldParent.RemoveChild(this);
        }
        _parent = parent;
        (parent as IParentNode)?.AddChild(this);
    }

    public override string ToString() {
        return $"{Name} ({ID}/{GetType().Name})";
    }

    public void Save(List<SavedData> savedNodes) {
        savedNodes.Add(WriteSavedData());
        if (this is IParentNode parent) {
            foreach (Node child in parent.Children) {
                child.Save(savedNodes);
            }
        }
    }
    protected virtual SavedData WriteSavedData() {
        return SavedData.FromNode(this);
    }
    protected virtual void ReadSaveData(SavedData save) {
        IsDisabled = save.IsDisabled;
    }

    [MessagePackObject]
    public record SavedData(Type NodeType, string ID, string Name, string? ParentID, bool IsDisabled) {
        public Dictionary<string, object?> Data { get; } = new();
        public static SavedData FromNode(Node node) {
            return new(node.GetType(), node.ID, node.Name, node.Parent?.ID, node.IsDisabled);
        }
        public Node LoadNode(Dictionary<string, Node> loadedNodes) {
            var node = (Node)Activator.CreateInstance(NodeType, new object[] { ID, Name })!;
            node.ReadSaveData(this);
            loadedNodes.Add(ID, node);
            if (!string.IsNullOrEmpty(ParentID)) {
                if (loadedNodes.TryGetValue(ParentID, out Node? parent) && parent is IParentNode) {
                    node.Parent = parent;
                } else {
                    BehaviorTree.Logger.Warn($"failed binding saved parent - parent node not loaded yet or invalid", node);
                }
            }
            return node;
        }
    }
}

public abstract class Node<TContext> : Node where TContext : NodeContext {
    protected Node() {
    }

    protected Node(string name) : base(name) {
    }

    protected Node(string id, string name) : base(id, name) {
    }

    public new TContext? NodeContext => Context?.GetNodeContext<TContext>(this);
}