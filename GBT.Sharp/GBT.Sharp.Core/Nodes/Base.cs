using NanoidDotNet;

namespace GBT.Sharp.Core.Nodes;


public abstract class BaseNode : INode {
    public string ID { get; }
    public string Name { get; set; }

    public NodeState State { get; set; }

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

    public virtual void Initialize() { }

    public void Tick() {
        if (Context is null) {
            TreeLogger.Error("this node has no context", this);
            return;
        }
        if (IsDisabled) {
            return;
        }
        if (State != NodeState.Running) {
            Context.CurrentTrace.Add(this);
            Initialize();
            Context.Tree.SetRunningNode(this);
        }
        State = NodeState.Running;
        DoTick();
        TryExit();
    }
    protected abstract void DoTick();

    public void TryExit() {
        if (State != NodeState.Running) {
            CleanUp();
            Context?.Tree.ExitRunningNode(this);
        }
    }

    public virtual void CleanUp() { }
    public virtual void Reset() {
        if (State != NodeState.Unvisited) {
            CleanUp();
        }
        State = NodeState.Unvisited;
    }
    protected virtual void OnContextUpdated() { }

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
}