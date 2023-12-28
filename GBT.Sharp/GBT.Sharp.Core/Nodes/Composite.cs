namespace GBT.Sharp.Core.Nodes;
/// <summary>
/// A Composite Node is a node that can have multiple children.
/// The node controls the flow of execution of its children in a specific way.
/// </summary>
public interface ICompositeNode : IParentNode {
    IEnumerable<BaseNode> Children { get; }
    BaseNode? CurrentChild { get; }
}

/// <summary>
/// This is the base class for all composite nodes who manages its children in a list.
/// With this manner, the children can be accessed by index, which can be convenient for
/// sequential execution or random access.
/// </summary>
public abstract class ListCompositeNode : BaseNode, ICompositeNode {
    protected List<BaseNode> _children = new();
    protected int _currentChildIndex = -1;
    public IEnumerable<BaseNode> Children => _children;
    public BaseNode? CurrentChild {
        get {
            if (_currentChildIndex < 0 || _currentChildIndex >= _children.Count) {
                return null;
            }

            BaseNode? child = _children[_currentChildIndex];
            if (child.IsDisabled) {
                // if the current child is disabled, recursively get the next enabled child or quit
                //_currentChildIndex++;
                //child = CurrentChild;
                child = CurrentChild = PeekNextChild();
            }
            return child;
        }
        protected set {
            if (value == null || value.IsDisabled) {
                _currentChildIndex = -1;
            } else {
                _currentChildIndex = _children.IndexOf((BaseNode)value);
            }
        }
    }

    public ListCompositeNode(string id, string name) : base(id, name) {
    }

    protected ListCompositeNode(string name) : base(name) {
    }

    protected ListCompositeNode() {
    }

    public override void Initialize() {
        base.Initialize();
        _currentChildIndex = 0;
        BehaviorTree.Logger.Info("initialized", this);
    }
    protected sealed override void DoTick() {
        BaseNode? child = CurrentChild;
        if (child is null) {
            Context?.Trace.Add(this, $"no current child");
            State = NodeState.Failure;
            BehaviorTree.Logger.Error($"failed due to no valid child node on index {_currentChildIndex}", this);
            return;
        }
        child.Tick();
    }
    public void OnChildExit(BaseNode child) {
        if (child != CurrentChild) {
            Context?.Trace.Add(this, $"skip: child exit");
            BehaviorTree.Logger.Warn($"skip: try to exit child {child} but the current child is {CurrentChild}", child);
            return;
        }
        AfterChildExit(child);
        TryExit();
    }
    protected abstract void AfterChildExit(BaseNode child);
    protected override void OnContextChanged() {
        base.OnContextChanged();
        if (State == NodeState.Running) {
            BehaviorTree.Logger.Info($"running node is reset because context is updated", this);
            Reset();
        }
        foreach (BaseNode child in _children) {
            child.Context = Context;
        }
    }
    public override void Reset() {
        base.Reset();
        _currentChildIndex = 0;
    }

    public void AddChild(BaseNode child) {
        if (!_children.Contains(child)) {
            _children.Add(child);
            child.Context = Context;
        }
        if (child.Parent != this) {
            // In case AddChild is not called from child.SetParent()
            child.Parent = this;
        }
    }

    public bool RemoveChild(BaseNode child) {
        var removed = _children.Remove(child);
        if (removed) {
            child.Context = null;
        }
        return removed;
    }
    protected BaseNode? PeekNextChild() {
        var nextIndex = _currentChildIndex + 1;
        BaseNode? nextChild = null;
        while (nextIndex < _children.Count && (nextChild = _children[nextIndex]).IsDisabled) {
            nextIndex++;
        };
        if (nextIndex >= _children.Count) return null;
        return nextChild;
    }
}

/// <summary>
/// SequenceNode executes its children sequentially until one of them fails,
/// analogous to the logical AND operator.
/// </summary>
public class SequenceNode : ListCompositeNode {
    public SequenceNode() {
    }

    public SequenceNode(string name) : base(name) {
    }

    public SequenceNode(string id, string name) : base(id, name) {
    }

    protected override void AfterChildExit(BaseNode child) {
        switch (child.State) {
            case NodeState.Running:
                State = NodeState.Running;
                break;
            case NodeState.Success:
                CurrentChild = PeekNextChild();
                State = CurrentChild is null ? NodeState.Success : NodeState.Running;
                break;
            case NodeState.Failure:
                CurrentChild = null;
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
    public SelectorNode() {
    }

    public SelectorNode(string name) : base(name) {
    }

    public SelectorNode(string id, string name) : base(id, name) {
    }

    protected override void AfterChildExit(BaseNode child) {
        switch (child.State) {
            case NodeState.Running:
                State = NodeState.Running;
                break;
            case NodeState.Success:
                CurrentChild = null;
                State = NodeState.Success;
                break;
            case NodeState.Failure:
                CurrentChild = PeekNextChild();
                State = CurrentChild is null ? NodeState.Failure : NodeState.Running;
                break;
            default:
                break;
        }
    }
}