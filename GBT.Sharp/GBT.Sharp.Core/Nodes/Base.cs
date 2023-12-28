using NanoidDotNet;

namespace GBT.Sharp.Core.Nodes;


public abstract class BaseNode : INode {
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

    public bool IsDisabled { get; set; }

    private ITreeContext? _context;
    public ITreeContext? Context {
        get => _context;
        set {
            if (_context != value) {
                _context = value;
                OnContextUpdated();
            }
        }
    }

    private IParentNode? _parent;
    public IParentNode? Parent { get => _parent; set => SetParent(value); }

    public BaseNode(string id, string name) {
        ID = id;
        Name = name;
        Reset();
    }
    public BaseNode(string name) : this(Nanoid.Generate(), name) {
        ID = Nanoid.Generate();
    }
    public BaseNode() : this("Unnamed") { }

    public virtual void Initialize() {
        Context?.Trace.Add(this, "initialize");
    }

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
            Context.Tree.SetRunningNode(this);
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
                Context?.Tree.ExitRunningNode(this);
            }
        }
    }

    public virtual void CleanUp() {
        Context?.Trace.Add(this, "clean up");
    }
    public virtual void Reset() {
        Context?.Trace.Add(this, "reset");
        if (State != NodeState.Unvisited) {
            CleanUp();
        }
        State = NodeState.Unvisited;
    }
    protected virtual void OnContextUpdated() {
    }

    private void SetParent(IParentNode? parent) {
        if (parent == Parent) {
            return;
        }

        if (Parent is not null and IParentNode oldParent) {
            oldParent.RemoveChild(this);
        }
        _parent = parent;
        parent?.AddChild(this);
    }

    public override string ToString() {
        return $"{Name} ({ID}/{GetType().Name})";
    }
}