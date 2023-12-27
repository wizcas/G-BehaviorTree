﻿namespace GBT.Sharp.Core.Nodes;

/// <summary>
/// A Decorator Node is a node that can have only one child.
/// </summary>
public interface IDecoratorNode : IParentNode {
    INode? Child { get; }
}

public abstract class DecoratorNode : BaseNode, IDecoratorNode {
    public DecoratorNode(string id, string name) : base(id, name) {
    }

    protected DecoratorNode(string name) : base(name) {
    }

    protected DecoratorNode() {
    }

    public INode? Child { get; private set; }
    public void AddChild(INode child) {
        if (child == Child) {
            return;
        }
        if (Child is not null) {
            RemoveChild(Child);
        }
        Child = child;
        Child.Context = Context;
        if (child.Parent != this) {
            child.Parent = this;
        }
    }

    public bool RemoveChild(INode child) {
        if (Child != child) {
            return false;
        }

        Child.Context = null;
        Child = null;
        return true;
    }

    public void OnChildExit(INode child) {
        if (child != Child) {
            BehaviorTree.Logger.Warn($"skip reacting on exit child {child} because it doesn't match the actual child {Child}", child);
            return;
        }
        AfterChildExit(child);
        TryExit();
    }
    protected abstract void AfterChildExit(INode child);

    protected sealed override void DoTick() {
        if (Child is null) {
            State = NodeState.Failure;
            BehaviorTree.Logger.Error("failed for no child is attached", this);
            return;
        }
        DoTick(Child);
    }
    protected virtual void DoTick(INode child) { child.Tick(); }
}

/// <summary>
/// Inverts the result of the child node, 
/// if it's <see cref="NodeState.Success"/> or <see cref="NodeState.Failure"/>.
/// </summary>
public class InverterNode : DecoratorNode {
    public InverterNode() {
    }

    public InverterNode(string name) : base(name) {
    }

    public InverterNode(string id, string name) : base(id, name) {
    }

    protected override void AfterChildExit(INode child) {
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
    public SucceederNode() {
    }

    public SucceederNode(string name) : base(name) {
    }

    public SucceederNode(string id, string name) : base(id, name) {
    }

    protected override void AfterChildExit(INode child) {
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

    public RepeaterNode(string name) : base(name) {
    }

    public RepeaterNode() {
    }

    public override void Initialize() {
        base.Initialize();
        _currentTimes = 0;
    }
    protected override void DoTick(INode child) {
        if (ShouldStop()) {
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

    protected override void AfterChildExit(INode child) {
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
public class RepeatUntilFailureNode : DecoratorNode {
    public RepeatUntilFailureNode() {
    }

    public RepeatUntilFailureNode(string name) : base(name) {
    }

    public RepeatUntilFailureNode(string id, string name) : base(id, name) {
    }

    protected override void AfterChildExit(INode child) {
        State = child.State switch {
            NodeState.Success => NodeState.Running,
            NodeState.Failure => NodeState.Success,
            _ => child.State,
        };
    }
}