using GBT.Sharp.Core.Diagnostics;
using GBT.Sharp.Core.Nodes;
using System.Buffers;

namespace GBT.Sharp.Core;


public class TreeContext {
    public BehaviorTree Tree { get; init; }
    public Trace Trace { get; } = new();
    private Node? _runningNode;
    /// <summary>
    /// The node that is currently running if during a tick,
    /// or the node that will run next if the previous tick is finished.
    /// </summary>
    public Node? RunningNode {
        get => _runningNode;
        set {
            if (_runningNode == value) {
                return;
            }
            Trace.Add(value, value is null ? "Running node cleared" : $"becomes running node");
            _runningNode = value;
            // Lazy-set node context in the tree
            if (value is not null) {
                lock (NodeContexts) {
                    NodeContexts[value.ID] = value.NodeContext;
                }
            }
        }
    }

    public Dictionary<string, NodeContext> NodeContexts { get; } = new();

    public TreeContext(BehaviorTree tree) {
        Tree = tree;
    }

    /// <summary>
    /// Save the context to a persistent storage.
    /// </summary>
    public Task Save(IBufferWriter<byte> writer) {
        // TODO
        return Task.CompletedTask;
    }
}

public class NodeContext {
    public Node Node { get; set; }

    private NodeState _state;
    public NodeState State {
        get => _state;
        set {
            if (_state != value) {
                Node.Context?.Trace.Add(Node, $"state change: {_state} -> {value}");
            }
            _state = value;
        }
    }

    public NodeContext(Node node) {
        Node = node;
    }
    public virtual void Reset() { }
}
public class NodeContext<T> : NodeContext where T : Node {
    public NodeContext(T node) : base(node) {
    }
    public new T Node => (T)base.Node;
}