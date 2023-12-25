namespace GBT.Sharp.Core.Nodes;


public interface INode {
    string ID { get; }
    string Name { get; }
    NodeState State { get; }
    bool IsEnabled { get; }
    ITreeContext? Context { get; set; }

    void Initialize();
    void Tick();
    void Exit();
}

public interface IParentNode : INode {
    void AttachChild(INode child);
    bool DetachChild(INode child);
}

public interface ILeafNode : INode { }

public enum NodeState {
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

    public BaseNode(string id, string name) {
        ID = id;
        Name = name;
    }

    public virtual void Initialize() { }

    public void Tick() {
        if (State != NodeState.Running) {
            Initialize();
        }
        State = NodeState.Running;
        DoTick();
        if (State != NodeState.Running) {
            Exit();
        }
    }
    public abstract void DoTick();

    public virtual void Exit() {
    }

    protected void SetState(NodeState state, string reason = "") {
        State = state;
        // TODO: Log reason in the trace
    }
    protected virtual void OnContextUpdated() { }
}
