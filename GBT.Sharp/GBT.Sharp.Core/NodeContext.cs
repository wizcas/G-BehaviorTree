using GBT.Sharp.Core.Exceptions;
using GBT.Sharp.Core.Nodes;
using MessagePack;

namespace GBT.Sharp.Core;

public class NodeContext {
    public Node Node { get; set; }

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

    public NodeContext(Node node) {
        Node = node;
    }
    public virtual void Reset() { }

    protected virtual Data WriteSavedData() {
        return Data.From(this);
    }
    protected virtual void ReadSavedData(Data data) {
        State = data.State;
    }


    [MessagePackObject(true)]
    public record struct Data(string NodeID, NodeState State) {
        public static Data From(NodeContext context) {
            return new(context.Node.ID, context.State);
        }
        public static NodeContext Load(Data data, IDictionary<string, Node> loadedNodes) {
            if (loadedNodes.TryGetValue(data.NodeID, out Node? node)) {
                NodeContext context = node.Context;
                context.ReadSavedData(data);
                return context;
            } else {
                throw new NodeNotFoundException(data.NodeID, "failed loading node context");
            }
        }
    }
}

public class NodeContext<T> : NodeContext where T : Node {
    public NodeContext(T node) : base(node) {
    }
    public new T Node => (T)base.Node;
}