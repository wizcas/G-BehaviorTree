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

    public NodeContext NodeContext { get; init; }

    /// <summary>
    /// A quick accessor to <see cref="NodeContext.State"/>.
    /// </summary>
    public NodeState State {
        get => NodeContext.State;
        set => NodeContext.State = value;
    }
    protected bool CanExit => Context?.RunningNode == this && State != NodeState.Running;

    public Node(string id, string name) {
        ID = id;
        Name = name;
        NodeContext = CreateNodeContext();
        Reset();
    }
    public Node(string name) : this(Nanoid.Generate(), name) { }
    public Node() : this("") {
        Name = $"New {GetType().Name}";
    }

    protected virtual NodeContext CreateNodeContext() {
        return new NodeContext(this);
    }

    protected void Enter() {
        if (Context is not null) {
            Context.Trace.Add(this, "enter");
            Initialize();
            Context.RunningNode = this;
        }
    }

    /// <summary>
    /// Set the node ready for running.
    /// This method is for internal purpose.
    /// </summary>
    protected virtual void Initialize() {
    }

    /// <summary>
    /// Called everytime the node is executed.
    /// </summary>
    public void Tick() {
        if (Context is null) {
            State = NodeState.Unvisited;
            BehaviorTree.Logger.Error("this node has no context", this);
            return;
        }
        if (IsDisabled) {
            State = NodeState.Unvisited;
            Context.Trace.Add(this, "skip: disabled");
            return;
        }
        if (State != NodeState.Running) {
            Enter();
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
        // Need to check if the node is still to be exited, 
        // because if this is a parent node and its child will exit within this tick,
        // TryExit() will be called twice - once when the child exits, and once in the parent's Tick().
        if (!CanExit) {
            return;
        }
        Context!.Trace.Add(this, "exit");
        CleanUp();
        if (State != NodeState.Unvisited) {
            Context!.RunningNode = Parent;
            (Parent as IParentNode)?.AfterChildExit(this);
        }
    }
    /// <summary>
    /// Clean up any intermediate state or data that was set
    /// by running this node.
    /// </summary>
    protected virtual void CleanUp() {
    }
    /// <summary>
    /// Reset this node to its initial state and data.
    /// </summary>
    public virtual void Reset() {
        Context?.Trace.Add(this, "reset");
        if (State != NodeState.Unvisited) {
            CleanUp();
            NodeContext.Reset();
            State = NodeState.Unvisited;
        }
    }
    protected virtual void OnContextChanged() { }

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

    public new TContext NodeContext => (TContext)base.NodeContext;

    protected override TContext CreateNodeContext() {
        return (TContext)Activator.CreateInstance(typeof(TContext), this)!;
    }
}