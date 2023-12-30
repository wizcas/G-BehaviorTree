﻿namespace GBT.Sharp.Core.Nodes;

/// <summary>
/// This is the base class for all composite nodes who manages its children in a list.
/// With this manner, the children can be accessed by index, which can be convenient for
/// sequential execution or random access.
/// </summary>
public abstract class ListCompositeNode : Node<ListCompositeNode.Ctx>, IParentNode {
    private readonly List<GBTNode> _children = new();
    public IEnumerable<GBTNode> Children => _children;

    public ListCompositeNode(string id, string name) : base(id, name) {
    }

    protected ListCompositeNode(string name) : base(name) {
    }

    protected ListCompositeNode() {
    }

    protected override void Initialize() {
        base.Initialize();
        Context.GoToNextChild();
        BehaviorTree.Logger.Info("initialized", this);
    }
    protected sealed override void DoTick() {
        GBTNode? child = Context.CurrentChild;
        if (child is null) {
            Runtime?.Trace.Add(this, $"no current child");
            State = NodeState.Failure;
            BehaviorTree.Logger.Error($"cannot tick because current child is empty", this);
            return;
        }
        child.Tick();
    }
    public void AfterChildExit(GBTNode child) {
        if (child != Context.CurrentChild) {
            Runtime?.Trace.Add(this, $"skip: child exit");
            BehaviorTree.Logger.Warn($"skip: try to exit child {child} but the current child is {Context.CurrentChild}", child);
            return;
        }
        ProceedChildState(child);
        TryExit();
    }
    protected abstract void ProceedChildState(GBTNode child);
    protected override void OnContextChanged() {
        base.OnContextChanged();
        if (State == NodeState.Running) {
            BehaviorTree.Logger.Info($"running node is reset because context is updated", this);
            Reset();
        }
        foreach (GBTNode child in _children) {
            child.Runtime = Runtime;
        }
    }
    public override void Reset() {
        base.Reset();
        Context.ResetCurrentChild();
    }


    public IParentNode AddChild(GBTNode child) {
        if (!_children.Contains(child)) {
            _children.Add(child);
            child.Runtime = Runtime;
        }
        if (child.Parent != this) {
            // In case AddChild is not called from child.SetParent()
            child.Parent = this;
        }
        return this;
    }

    public IParentNode AddChildren(params GBTNode[] children) {
        foreach (GBTNode child in children) {
            AddChild(child);
        }
        return this;
    }


    public bool RemoveChild(GBTNode child) {
        var removed = _children.Remove(child);
        if (removed) {
            child.Runtime = null;
        }
        return removed;
    }


    public class Ctx : NodeContext<ListCompositeNode> {
        private int _currentChildIndex = -1;
        private List<GBTNode> Children => Node._children;
        public GBTNode? CurrentChild {
            get {
                if (_currentChildIndex < 0 || _currentChildIndex >= Children.Count) {
                    return null;
                }

                GBTNode? child = Children[_currentChildIndex];
                if (child.IsDisabled) {
                    // if the current child is disabled, recursively get the next enabled child or quit
                    child = CurrentChild = PeekNextChild();
                }
                return child;
            }
            private set {
                if (value == null || value.IsDisabled) {
                    _currentChildIndex = -1;
                } else {
                    _currentChildIndex = Children.IndexOf(value);
                }
            }
        }
        public Ctx(ListCompositeNode node) : base(node) {
        }
        public override void Reset() {
            base.Reset();
            ResetCurrentChild();
        }
        public void ResetCurrentChild() {
            CurrentChild = null;
        }
        public GBTNode? PeekNextChild() {
            var nextIndex = _currentChildIndex + 1;
            GBTNode? nextChild = null;
            while (nextIndex < Children.Count && (nextChild = Children[nextIndex]).IsDisabled) {
                nextIndex++;
            };
            return nextIndex >= Children.Count ? null : nextChild;
        }
        public GBTNode? GoToNextChild() {
            CurrentChild = PeekNextChild();
            return CurrentChild;
        }
    }
}

public abstract class ListCompositeNode<TNode> : ListCompositeNode, IParentNode<TNode> where TNode : GBTNode {
    public new IEnumerable<TNode> Children => base.Children.Cast<TNode>();
    public ListCompositeNode(string id, string name) : base(id, name) {
    }

    protected ListCompositeNode(string name) : base(name) {
    }

    protected ListCompositeNode() {
    }

    public new TNode AddChild(GBTNode child) {
        return base.AddChild(child).Cast<TNode>();
    }

    public new TNode AddChildren(params GBTNode[] children) {
        return base.AddChildren(children).Cast<TNode>();
    }
}

/// <summary>
/// SequenceNode executes its children sequentially until one of them fails,
/// analogous to the logical AND operator.
/// </summary>
public class SequenceNode : ListCompositeNode<SequenceNode> {
    public SequenceNode() {
    }

    public SequenceNode(string name) : base(name) {
    }

    public SequenceNode(string id, string name) : base(id, name) {
    }

    protected override void ProceedChildState(GBTNode child) {
        switch (child.State) {
            case NodeState.Running:
                State = NodeState.Running;
                break;
            case NodeState.Success:
                Context.GoToNextChild();
                State = Context.CurrentChild is null ? NodeState.Success : NodeState.Running;
                break;
            case NodeState.Failure:
                Context.ResetCurrentChild();
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
public class SelectorNode : ListCompositeNode<SelectorNode> {
    public SelectorNode() {
    }

    public SelectorNode(string name) : base(name) {
    }

    public SelectorNode(string id, string name) : base(id, name) {
    }

    protected override void ProceedChildState(GBTNode child) {
        switch (child.State) {
            case NodeState.Running:
                State = NodeState.Running;
                break;
            case NodeState.Success:
                Context.ResetCurrentChild();
                State = NodeState.Success;
                break;
            case NodeState.Failure:
                Context.GoToNextChild();
                State = Context.CurrentChild is null ? NodeState.Failure : NodeState.Running;
                break;
            default:
                break;
        }
    }
}