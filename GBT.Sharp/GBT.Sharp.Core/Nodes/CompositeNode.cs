namespace GBT.Sharp.Core.Nodes;
/// <summary>
/// A Composite Node is a node that can have multiple children.
/// The node controls the flow of execution of its children in a specific way.
/// </summary>
public interface ICompositeNode : IParentNode
{
    IEnumerable<INode> Children { get; }
    INode? CurrentChild { get; }
}

/// <summary>
/// This is the base class for all composite nodes who manages its children in a list.
/// With this manner, the children can be accessed by index, which can be convenient for
/// sequential execution or random access.
/// </summary>
public abstract class ListCompositeNode : BaseNode, ICompositeNode
{
    protected List<INode> _children = new();
    protected int _currentChildIndex = -1;
    public IEnumerable<INode> Children => _children;
    public INode? CurrentChild
    {
        get => _currentChildIndex >= 0 && _currentChildIndex < _children.Count ? _children[_currentChildIndex] : null;
        protected set
        {
            if (value == null)
            {
                _currentChildIndex = -1;
                return;
            }
            _currentChildIndex = _children.IndexOf(value);
        }
    }
    public ListCompositeNode(string id, string name) : base(id, name)
    {
    }
    public override void Initialize()
    {
        base.Initialize();
        _currentChildIndex = 0;
    }
    public override void TickInternal()
    {
        INode? child = CurrentChild;
        if (child is null)
        {
            SetState(NodeState.Failure, $"no valid child node on index {_currentChildIndex}");
            return;
        }
        child.Tick();
        AfterChildTick(child);
    }
    protected abstract void AfterChildTick(INode child);
    protected override void OnContextUpdated()
    {
        base.OnContextUpdated();
        foreach (INode child in _children)
        {
            child.Context = Context;
        }
    }

    public void AttachChild(INode child)
    {
        _children.Add(child);
        child.Context = Context;
    }

    public bool DetachChild(INode child)
    {
        if (_children.Contains(child))
        {
            return _children.Remove(child);
        }
        return false;
    }
}

/// <summary>
/// SequenceNode executes its children sequentially until one of them fails,
/// analogous to the logical AND operator.
/// </summary>
public class SequenceNode : ListCompositeNode
{
    public SequenceNode(string id, string name) : base(id, name)
    {
    }

    protected override void AfterChildTick(INode child)
    {
        if (child.State == NodeState.Running)
        {
            SetState(NodeState.Running);
            return;
        }
        if (child.State == NodeState.Failure)
        {
            SetState(NodeState.Failure, $"child node has failed");
            return;
        }
        if (child.State == NodeState.Success)
        {
            _currentChildIndex++;
            SetState(_currentChildIndex >= _children.Count ? NodeState.Success : NodeState.Running);
        }
    }
}

/// <summary>
/// SelectorNode executes its children sequentially until one of them succeeds,
/// analogous to the logical OR operator.
/// </summary>
public class SelectorNode : ListCompositeNode
{
    public SelectorNode(string id, string name) : base(id, name)
    {
    }

    protected override void AfterChildTick(INode child)
    {
        if (child.State == NodeState.Running)
        {
            SetState(NodeState.Running);
            return;
        }
        if (child.State == NodeState.Success)
        {
            SetState(NodeState.Success);
            return;
        }
        if (child.State == NodeState.Failure)
        {
            _currentChildIndex++;
            SetState(_currentChildIndex >= _children.Count ? NodeState.Failure : NodeState.Running);
        }
    }
}
