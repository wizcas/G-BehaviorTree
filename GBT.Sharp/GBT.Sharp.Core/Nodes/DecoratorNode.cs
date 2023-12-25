namespace GBT.Sharp.Core.Nodes;

public interface IDecoratorNode : IParentNode {
    INode? Child { get; }
}
public abstract class DecoratorNode : BaseNode, IDecoratorNode {
    public DecoratorNode(string id, string name) : base(id, name) {
    }

    private INode? _child;
    public INode? Child => _child;
    public void AttachChild(INode child) {
        if (_child is not null) {
            DetachChild(_child);
        }
        _child = child;
        _child.Context = Context;
    }

    public bool DetachChild(INode child) {
        if (_child != child) return false;
        _child.Context = null;
        _child = null;
        return true;
    }

    public override void DoTick() {
        if (_child is null) {
            SetState(NodeState.Failure, $"no valid child node");
            return;
        }
        DoTick(_child);
    }
    protected abstract void DoTick(INode child);
}

public class InverterNode : DecoratorNode {
    public InverterNode(string id, string name) : base(id, name) {
    }

    protected override void DoTick(INode child) {
        child.Tick();
        switch (child.State) {
            case NodeState.Success:
                SetState(NodeState.Failure);
                break;
            case NodeState.Failure:
                SetState(NodeState.Success);
                break;
            default:
                SetState(child.State);
                break;
        }
    }
}

public class SucceederNode : DecoratorNode {
    public SucceederNode(string id, string name) : base(id, name) {
    }

    protected override void DoTick(INode child) {
        child.Tick();
        SetState(NodeState.Success);
    }
}

public class RepeaterNode : DecoratorNode {
    public int Times { get; set; } = -1;
    private int _currentTimes = 0;
    public RepeaterNode(string id, string name) : base(id, name) {
    }

    protected override void DoTick(INode child) {
        if (Times >= 0 && _currentTimes >= Times) {
            SetState(NodeState.Success);
            return;
        }
        SetState(NodeState.Running);
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

public class RepeatUntilFailureNode : DecoratorNode {
    public RepeatUntilFailureNode(string id, string name) : base(id, name) {
    }

    protected override void DoTick(INode child) {
        child.Tick();
        switch (child.State) {
            case NodeState.Success:
                SetState(NodeState.Running);
                break;
            case NodeState.Failure:
                SetState(NodeState.Success);
                break;
            default:
                SetState(child.State);
                break;
        }
    }
}
