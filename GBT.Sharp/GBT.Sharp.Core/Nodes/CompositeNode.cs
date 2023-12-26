namespace GBT.Sharp.Core.Nodes;
/// <summary>
/// A Composite Node is a node that can have multiple children.
/// The node controls the flow of execution of its children in a specific way.
/// </summary>
public interface ICompositeNode : IParentNode {
    IEnumerable<INode> Children { get; }
    INode? CurrentChild { get; }
}

/// <summary>
/// This is the base class for all composite nodes who manages its children in a list.
/// With this manner, the children can be accessed by index, which can be convenient for
/// sequential execution or random access.
/// </summary>
public abstract class ListCompositeNode : BaseNode, ICompositeNode {
    protected List<INode> _children = new();
    protected int _currentChildIndex = -1;
    public IEnumerable<INode> Children => _children;
    public INode? CurrentChild {
        get {
            if (_currentChildIndex < 0 || _currentChildIndex >= _children.Count) return null;
            INode? child = _children[_currentChildIndex];
            if (child.IsDisabled) {
                // if the current child is disabled, recursively get the next enabled child or quit
                _currentChildIndex++;
                child = CurrentChild;
            }
            return child;
        }
        protected set {
            if (value == null || value.IsDisabled) {
                _currentChildIndex = -1;
                return;
            }
            _currentChildIndex = _children.IndexOf(value);
        }
    }
    public ListCompositeNode(string id, string name) : base(id, name) {
    }
    public override void Initialize() {
        base.Initialize();
        _currentChildIndex = 0;
        TreeLogger.Info("initialized", this);
    }
    protected sealed override void DoTick() {
        INode? child = CurrentChild;
        if (child is null) {
            State = NodeState.Failure;
            TreeLogger.Error($"failed due to no valid child node on index {_currentChildIndex}", this);
            return;
        }
        child.Tick();
        OnChildExit(child);
    }
    protected abstract void OnChildExit(INode child);
    protected override void OnContextUpdated() {
        base.OnContextUpdated();
        if (State == NodeState.Running) {
            TreeLogger.Info($"running node is re-initialized because context is updated", this);
            Initialize();
        }
        foreach (INode child in _children) {
            child.Context = Context;
        }
    }

    public void AttachChild(INode child) {
        _children.Add(child);
        child.Context = Context;
    }

    public bool DetachChild(INode child) {
        if (_children.Contains(child)) {
            return _children.Remove(child);
        } else {
            return false;
        }
    }
}

/// <summary>
/// SequenceNode executes its children sequentially until one of them fails,
/// analogous to the logical AND operator.
/// </summary>
public class SequenceNode : ListCompositeNode {
    public SequenceNode(string id, string name) : base(id, name) {
    }

    protected override void OnChildExit(INode child) {
        switch (child.State) {
            case NodeState.Running:
                State = NodeState.Running;
                break;
            case NodeState.Success:
                _currentChildIndex++;
                State = _currentChildIndex >= _children.Count ? NodeState.Success : NodeState.Running;
                break;
            case NodeState.Failure:
                State = NodeState.Failure;
                break;
            default:
                break;
        }
    }
}

/// <summary>
/// SelectorNode executes its children sequentially until one of them succeeds,
/// analogous to the logical OR operator.
/// </summary>
public class SelectorNode : ListCompositeNode {
    public SelectorNode(string id, string name) : base(id, name) {
    }

    protected override void OnChildExit(INode child) {
        switch (child.State) {
            case NodeState.Running:
                State = NodeState.Running;
                break;
            case NodeState.Success:
                State = NodeState.Success;
                break;
            case NodeState.Failure:
                _currentChildIndex++;
                State = _currentChildIndex >= _children.Count ? NodeState.Failure : NodeState.Running;
                break;
            default:
                break;
        }
    }
}