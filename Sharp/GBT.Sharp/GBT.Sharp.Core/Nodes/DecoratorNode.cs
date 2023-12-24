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

    public override void TickInternal() {
        if (_child is null) {
            SetState(NodeState.Failure, $"no valid child node");
            return;
        }
        DoTick(_child);
    }
    protected abstract void DoTick(INode child);
}
