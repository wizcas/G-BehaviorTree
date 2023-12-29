using GBT.Sharp.Core.Diagnostics;
using GBT.Sharp.Core.Nodes;
using System.Buffers;

namespace GBT.Sharp.Core;


public class TreeContext {
    public BehaviorTree Tree { get; init; }
    public Trace Trace { get; } = new();
    /// <summary>
    /// The node that is currently running if during a tick,
    /// or the node that will run next if the previous tick is finished.
    /// </summary>
    public Node? RunningNode { get; set; }

    public Dictionary<string, NodeContext> NodeContexts { get; } = new();

    public TreeContext(BehaviorTree tree) {
        Tree = tree;
    }
    /// <summary>
    /// Called when a node begins to run for the first time in
    /// the current pass.
    /// </summary>
    public void EnterNode(Node? node) {
        Trace.Add(node, node is null ? "Running node cleared" : $"becomes running node");
        RunningNode = node;
    }
    /// <summary>
    /// Called when a node finishes running in the current pass by
    /// reaching a final state (success or failure).
    /// </summary>
    public bool ExitNode(Node node) {
        Trace.Add(node, $"exit");
        if (RunningNode != node) {
            BehaviorTree.Logger.Warn($"skip: try to exit running node {node} but the running node is {RunningNode}", node);
            return false;
        }
        return true;
    }

    /// <summary>
    /// Save the context to a persistent storage.
    /// </summary>
    public Task Save(IBufferWriter<byte> writer) {
        // TODO
        return Task.CompletedTask;
    }

    public T GetNodeContext<T>(Node node) where T : NodeContext {
        if (!NodeContexts.TryGetValue(node.ID, out NodeContext? context)
            || context is null
            || context is not T) {
            context = (T)Activator.CreateInstance(typeof(T), node, this)!;
            NodeContexts.Add(node.ID, context);
        }
        return (T)context;
    }
}

public class NodeContext {
    public Node Node { get; set; }
    public TreeContext TreeContext { get; set; }

    private NodeState _state;
    public NodeState State {
        get => _state;
        set {
            if (_state != value) {
                TreeContext.Trace.Add(Node, $"state change: {_state} -> {value}");
            }
            _state = value;
        }
    }

    public NodeContext(Node node, TreeContext treeContext) {
        Node = node;
        TreeContext = treeContext;
    }
}
public class NodeContext<T> : NodeContext where T : Node {
    public NodeContext(T node, TreeContext treeContext) : base(node, treeContext) {
    }
    public new T Node => (T)base.Node;
}