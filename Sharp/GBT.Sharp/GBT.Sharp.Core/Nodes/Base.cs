namespace GBT.Sharp.Core.Nodes;


public interface INode
{
    string ID { get; }
    string Name { get; }
    NodeState State { get; }
    bool IsEnabled { get; }
    ITreeContext? Context { get; set; }

    void Initialize();
    void Tick();
    void CleanUp();
}

public interface IParentNode : INode
{
    void AttachChild(INode child);
    bool DetachChild(INode child);
}

public interface ILeafNode : INode { }

public enum NodeState
{
    Ready,
    Running,
    Success,
    Failure,
}

public abstract class BaseNode : INode
{
    public string ID { get; }
    public string Name { get; set; }

    public NodeState State { get; private set; }

    public bool IsEnabled { get; set; }

    private ITreeContext? _context;
    public ITreeContext? Context
    {
        get => _context;
        set
        {
            if (_context != value)
            {
                _context = value;
                OnContextUpdated();
            }
        }
    }

    public BaseNode(string id, string name)
    {
        ID = id;
        Name = name;
    }

    public virtual void Initialize() { }

    public void Tick()
    {
        if (State == NodeState.Ready)
        {
            Initialize();
        }
        State = NodeState.Running;
        TickInternal();
        if (State != NodeState.Running)
        {
            CleanUp();
        }
    }
    public abstract void TickInternal();

    public void CleanUp()
    {
        State = NodeState.Ready;
        CleanUpInternal();
    }
    public virtual void CleanUpInternal() { }

    protected void SetState(NodeState state, string reason = "")
    {
        State = state;
        // TODO: Log reason in the trace
    }
    protected virtual void OnContextUpdated() { }
}
