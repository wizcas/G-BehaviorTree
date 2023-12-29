namespace GBT.Sharp.Core.Nodes;

/// <summary>
/// A Decorator Node is a node that can have only one child.
/// </summary>
public interface ISingularParentNode : IParentNode {
    Node? Child { get; }
}

public abstract class DecoratorNode : Node, ISingularParentNode {
    public DecoratorNode(string id, string name) : base(id, name) {
    }

    protected DecoratorNode(string name) : base(name) {
    }

    protected DecoratorNode() {
    }

    public Node? Child { get; private set; }
    public IEnumerable<Node> Children => Child is null ? Enumerable.Empty<Node>() : new[] { Child };
    public IParentNode AddChild(Node child) {
        if (child == Child) {
            return this;
        }
        if (Child is not null) {
            RemoveChild(Child);
        }
        Child = child;
        Child.Runtime = Runtime;
        if (child.Parent != this) {
            child.Parent = this;
        }
        return this;
    }
    public IParentNode AddChildren(params Node[] children) {
        if (children.Length > 1) {
            throw new InvalidOperationException("Decorator node can only have one child");
        }
        if (children.Length == 1) {
            AddChild(children[0]);
        }
        return this;
    }

    public bool RemoveChild(Node child) {
        if (Child != child) {
            return false;
        }

        Child.Runtime = null;
        Child = null;
        return true;
    }

    public void AfterChildExit(Node child) {
        if (child != Child) {
            Runtime?.Trace.Add(this, $"skip: child exit");
            BehaviorTree.Logger.Warn($"skip reacting on exit child {child} because it doesn't match the actual child {Child}", child);
            return;
        }
        ProceedChildState(child);
        TryExit();
    }
    protected abstract void ProceedChildState(Node child);

    protected sealed override void DoTick() {
        if (Child is null || Child.IsDisabled) {
            Runtime?.Trace.Add(this, $"no current child");
            State = NodeState.Failure;
            BehaviorTree.Logger.Error("failed for no child is available or enabled", this);
            return;
        }
        DoTick(Child);
    }
    protected virtual void DoTick(Node child) {
        child.Tick();
    }
}

public abstract class DecoratorNode<TNode> : DecoratorNode, IParentNode<TNode> where TNode : Node {
    protected DecoratorNode() {
    }

    protected DecoratorNode(string name) : base(name) {
    }

    protected DecoratorNode(string id, string name) : base(id, name) {
    }

    public new TNode? Child => base.Child as TNode;
    public new TNode AddChild(Node child) {
        return base.AddChild(child).Cast<TNode>();
    }
    public new TNode AddChildren(params Node[] children) {
        return base.AddChildren(children).Cast<TNode>();
    }
}

/// <summary>
/// Inverts the result of the child node, 
/// if it's <see cref="NodeState.Success"/> or <see cref="NodeState.Failure"/>.
/// </summary>
public class InverterNode : DecoratorNode<InverterNode> {
    public InverterNode() {
    }

    public InverterNode(string name) : base(name) {
    }

    public InverterNode(string id, string name) : base(id, name) {
    }

    protected override void ProceedChildState(Node child) {
        State = child.State switch {
            NodeState.Success => NodeState.Failure,
            NodeState.Failure => NodeState.Success,
            _ => child.State,
        };
    }
}

/// <summary>
/// SucceederNode always returns <see cref="NodeState.Success"/> regardless of the child node result.
/// </summary>
public class SucceederNode : DecoratorNode<SucceederNode> {
    public SucceederNode() {
    }

    public SucceederNode(string name) : base(name) {
    }

    public SucceederNode(string id, string name) : base(id, name) {
    }

    protected override void ProceedChildState(Node child) {
        State = NodeState.Success;
    }

}

/// <summary>
/// RepeaterNode repeats the child node execution. It has two modes:
/// <para>1. Infinite mode if <see cref="Times"/> is set to any negative integer. The child node will be
/// executed repeatedly as long as the node is still in the tree and enabled.
/// </para>
/// <para>2. Repeat by times if <see cref="Times"/> is set to a non-negative integer. The child node will
/// be executed repeatedly for the given times. Node that <c>0</c> times will forbid the child node to run.</para>
/// </summary>
public class RepeaterNode : DecoratorNode<RepeaterNode> {
    private int _currentTimes;
    /// <summary>
    /// How many times to run the child node.
    /// If the value is <c>0</c>, the child node won't run at all.
    /// If the value is negative, the child node will run infinitely.
    /// </summary>
    public int Times { get; set; } = -1;
    public RepeaterNode(string id, string name) : base(id, name) {
    }

    public RepeaterNode(string name) : base(name) {
    }

    public RepeaterNode() {
    }

    protected override void Initialize() {
        base.Initialize();
        _currentTimes = 0;
    }
    protected override void DoTick(Node child) {
        if (ShouldStop()) {
            Runtime?.Trace.Add(this, "repeat ends on target times");
            State = NodeState.Success;
            BehaviorTree.Logger.Warn($"repeater node not run at all: current times is {_currentTimes}, while the repeat times is {Times}", this);
            return;
        }
        State = NodeState.Running;
        child.Tick();
    }

    private bool ShouldStop() {
        return Times >= 0 && _currentTimes >= Times;
    }

    protected override void ProceedChildState(Node child) {
        if (child.State is NodeState.Success or NodeState.Failure) {
            _currentTimes++;
        }
        if (ShouldStop()) {
            State = NodeState.Success;
        }
    }
}

/// <summary>
/// RepeatUntilFailureNode repeats the child node execution until it fails.
/// </summary>
public class RepeatUntilFailureNode : DecoratorNode<RepeatUntilFailureNode> {
    public RepeatUntilFailureNode() {
    }

    public RepeatUntilFailureNode(string name) : base(name) {
    }

    public RepeatUntilFailureNode(string id, string name) : base(id, name) {
    }

    protected override void ProceedChildState(Node child) {
        State = child.State switch {
            NodeState.Success => NodeState.Running,
            NodeState.Failure => NodeState.Success,
            _ => child.State,
        };
    }
}