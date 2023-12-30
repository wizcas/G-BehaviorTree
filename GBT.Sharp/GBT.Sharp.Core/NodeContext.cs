using GBT.Sharp.Core.Nodes;

namespace GBT.Sharp.Core;

public partial class NodeContext {
    public GBTNode Node { get; set; }

    private NodeState _state;
    public NodeState State {
        get => _state;
        set {
            if (_state != value) {
                Node.Runtime?.Trace.Add(Node, $"state change: {_state} -> {value}");
            }
            _state = value;
        }
    }

    public NodeContext(GBTNode node) {
        Node = node;
    }
    public virtual void Reset() { }

    protected virtual Data WriteSavedData() {
        return Data.From(this);
    }
    protected virtual void ReadSavedData(Data data) {
        State = data.State;
    }
}

public class NodeContext<T> : NodeContext where T : GBTNode {
    public NodeContext(T node) : base(node) {
    }
    public new T Node => (T)base.Node;
}