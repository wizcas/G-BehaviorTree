namespace GBT.Sharp.Core.Nodes;

/// <summary>
/// A Decorator Node is a node that can have only one child.
/// </summary>
public interface IDecoratorNode : IParentNode {
    INode? Child { get; }
}

public abstract class DecoratorNode : BaseNode, IDecoratorNode {
    public DecoratorNode(string id, string name) : base(id, name) {
    }

    public INode? Child { get; private set; }
    public void AttachChild(INode child) {
        if (Child is not null) {
            DetachChild(Child);
        }
        Child = child;
        Child.Context = Context;
    }

    public bool DetachChild(INode child) {
        if (Child != child) {
            return false;
        }

        Child.Context = null;
        Child = null;
        return true;
    }

    public override void DoTick() {
        if (Child is null) {
            State = NodeState.Failure;
            TreeLogger.Error("failed for no child is attached", this);
            return;
        }
        DoTick(Child);
    }
    protected abstract void DoTick(INode child);
}

/// <summary>
/// Inverts the result of the child node, 
/// if it's <see cref="NodeState.Success"/> or <see cref="NodeState.Failure"/>.
/// </summary>
public class InverterNode : DecoratorNode {
    public InverterNode(string id, string name) : base(id, name) {
    }

    protected override void DoTick(INode child) {
        child.Tick();
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
public class SucceederNode : DecoratorNode {
    public SucceederNode(string id, string name) : base(id, name) {
    }

    protected override void DoTick(INode child) {
        child.Tick();
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
public class RepeaterNode : DecoratorNode {
    private int _currentTimes = 0;
    /// <summary>
    /// How many times to run the child node.
    /// If the value is <c>0</c>, the child node won't run at all.
    /// If the value is negative, the child node will run infinitely.
    /// </summary>
    public int Times { get; set; } = -1;
    public RepeaterNode(string id, string name) : base(id, name) {
    }

    protected override void DoTick(INode child) {
        if (Times >= 0 && _currentTimes >= Times) {
            State = NodeState.Success;
            return;
        }
        State = NodeState.Running;
        child.Tick();
        if (child.State is NodeState.Success or NodeState.Failure) {
            _currentTimes++;
        }
    }
    public override void Initialize() {
        base.Initialize();
        _currentTimes = 0;
    }
}

/// <summary>
/// RepeatUntilFailureNode repeats the child node execution until it fails.
/// </summary>
public class RepeatUntilFailureNode : DecoratorNode {
    public RepeatUntilFailureNode(string id, string name) : base(id, name) {
    }

    protected override void DoTick(INode child) {
        child.Tick();
        State = child.State switch {
            NodeState.Success => NodeState.Running,
            NodeState.Failure => NodeState.Success,
            _ => child.State,
        };
    }
}