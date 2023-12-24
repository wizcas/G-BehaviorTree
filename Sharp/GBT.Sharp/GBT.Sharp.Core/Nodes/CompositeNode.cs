namespace GBT.Sharp.Core.Nodes;
public interface ICompositeNode<TContext> : INode<TContext>
{
    IEnumerable<INode<TContext>> Children { get; }
    INode<TContext>? CurrentChild { get; }
}

public abstract class CompositeNode<TContext> : BaseNode<TContext>, ICompositeNode<TContext>
{
    public abstract IEnumerable<INode<TContext>> Children { get; protected set; }
    public abstract INode<TContext>? CurrentChild { get; protected set; }

    public CompositeNode(string id, string name) : base(id, name)
    {
    }
}

public class SequenceNode<TContext> : CompositeNode<TContext>
{
    protected List<INode<TContext>> _children = new();
    private int _currentChildIndex = 0;
    public override IEnumerable<INode<TContext>> Children
    {
        get => _children;
        protected set => _children = value.ToList();
    }
    public override INode<TContext>? CurrentChild
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

    public override TContext Context => throw new NotImplementedException();


    public SequenceNode(string id, string name) : base(id, name)
    {
    }

    public override void Initialize()
    {
        base.Initialize();
        _currentChildIndex = 0;
    }
    public override void TickInternal()
    {
        INode<TContext>? child = CurrentChild;
        if (child is null)
        {
            State = NodeState.Failure;
            return;
        }
        child.Tick();
        if (child.State == NodeState.Running)
        {
            State = NodeState.Running;
            return;
        }
        if (child.State == NodeState.Failure)
        {
            State = NodeState.Failure;
            return;
        }
        if (child.State == NodeState.Success)
        {
            _currentChildIndex++;
            State = _currentChildIndex >= _children.Count ? NodeState.Success : NodeState.Running;
        }
    }
}
