using GBT.Sharp.Core.Nodes;

namespace GBT.Sharp.Core;

public class NodeContext
{
    public Node Node { get; set; }

    private NodeState _state;
    public NodeState State
    {
        get => _state;
        set
        {
            if (_state != value)
            {
                Node.Runtime?.Trace.Add(Node, $"state change: {_state} -> {value}");
            }
            _state = value;
        }
    }

    public NodeContext(Node node)
    {
        Node = node;
    }
    public virtual void Reset() { }
}

public class NodeContext<T> : NodeContext where T : Node
{
    public NodeContext(T node) : base(node)
    {
    }
    public new T Node => (T)base.Node;
}