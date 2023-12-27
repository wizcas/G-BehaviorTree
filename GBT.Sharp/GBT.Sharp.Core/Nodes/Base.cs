namespace GBT.Sharp.Core.Nodes;


public interface INode {
    string ID { get; }
    string Name { get; }
    NodeState State { get; set; }
    bool IsDisabled { get; set; }
    IParentNode? Parent { get; }
    ITreeContext? Context { get; set; }

    void Initialize();
    void Tick();
    void CleanUp();
    void Reset();
    void SetParent(IParentNode? parent);
    void TryExit();
}

public interface IParentNode : INode {
    void AddChild(INode child);
    bool RemoveChild(INode child);
    void OnChildExit(INode child);
}

public interface ILeafNode : INode { }

public enum NodeState {
    Unvisited,
    Running,
    Success,
    Failure,
}

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

    public IParentNode? Parent { get; set; }

    public BaseNode(string id, string name) {
        ID = id;
        Name = name;
        Reset();
    }

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

    public void SetParent(IParentNode? parent) {
        if (parent == Parent) {
            return;
        }

        if (Parent is not null and IParentNode oldParent) {
            oldParent.RemoveChild(this);
        }
        Parent = parent;
        parent?.AddChild(this);
    }
}

public class CallbackNode : BaseNode {
    public Action<CallbackNode>? OnInitialize { get; set; }
    public Action<CallbackNode>? OnTick { get; set; }
    public Action<CallbackNode>? OnCleanUp { get; set; }
    public CallbackNode(string id, string name) : base(id, name) {
    }

    protected override void DoTick() {
        OnTick?.Invoke(this);
    }

    public override void Initialize() {
        base.Initialize();
        OnInitialize?.Invoke(this);
    }
    public override void CleanUp() {
        base.CleanUp();
        OnCleanUp?.Invoke(this);
    }
}