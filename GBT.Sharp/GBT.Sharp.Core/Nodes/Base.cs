namespace GBT.Sharp.Core.Nodes;


public interface INode {
    string ID { get; }
    string Name { get; }
    NodeState State { get; set; }
    bool IsDisabled { get; set; }
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

    public INode? Parent { get; set; }

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
    protected abstract void DoTick();

    public virtual void Exit() {
    }
    public virtual void Reset() {
        State = NodeState.Unvisited;
    }
    protected virtual void OnContextUpdated() { }
}

public class CallbackNode : BaseNode {
    public Action? Callback { get; set; }
    public CallbackNode(string id, string name) : base(id, name) {
    }

    protected override void DoTick() {
        Callback?.Invoke();
    }
}