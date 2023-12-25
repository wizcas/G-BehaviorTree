namespace GBT.Sharp.Core.Nodes;


public interface INode {
    string ID { get; }
    string Name { get; }
    NodeState State { get; }
    bool IsEnabled { get; }
    INode? Parent { get; }
    ITreeContext? Context { get; set; }

    void Initialize();
    void Tick();
    void Exit();
    void Reset();
}

public interface IParentNode : INode {
    void AttachChild(INode child);
    bool DetachChild(INode child);
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

    public NodeState State { get; private set; }

    public bool IsEnabled { get; set; }

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
    protected bool CanTick => Context is not null && IsEnabled;

    public INode? Parent { get; set; }

    public BaseNode(string id, string name) {
        ID = id;
        Name = name;
        Reset();
    }

    public virtual void Initialize() { }

    public void Tick() {
        if (Context is null) {
            SetState(NodeState.Failure);
            TreeLogger.Error("failed for no context", this);
            return;
        }
        if (!IsEnabled) {
            SetState(NodeState.Failure);
            return;
        }
        if (State != NodeState.Running) {
            Initialize();
            Context.Tree.SetRunningNode(this);
        }
        State = NodeState.Running;
        DoTick();
        if (State != NodeState.Running) {
            Exit();
            Context.Tree.ExitRunningNode(this);
        }
    }
    public abstract void DoTick();

    public virtual void Exit() {
    }
    public virtual void Reset() {
        State = NodeState.Unvisited;
    }

    protected void SetState(NodeState state) {
        State = state;
    }
    protected virtual void OnContextUpdated() { }
}
