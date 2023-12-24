namespace GBT.Sharp.Core.Nodes;

public interface INode<TContext>
{
    string ID { get; }
    string Name { get; }
    NodeState State { get; }
    bool IsEnabled { get; }
    TContext Context { get; }

    void Initialize();
    void Tick();
    void CleanUp();
}

public interface ILeafNode<TContext> : INode<TContext> { }

public enum NodeState
{
    Ready,
    Running,
    Success,
    Failure,
}

public abstract class BaseNode<TContext> : INode<TContext>
{
    public string ID { get; }
    public string Name { get; set; }

    public NodeState State { get; protected set; }

    public bool IsEnabled { get; set; }

    public abstract TContext Context { get; }

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
}
