using GBT.Sharp.Core.Diagnostics;
using GBT.Sharp.Core.Nodes;
using System.Buffers;

namespace GBT.Sharp.Core;

/// <summary>
/// The runtime context of a Behavior Tree, which contains the tree that it belongs to,
/// the current runtime state, and traces for diagnostics.
/// The TreeRuntime is independent to the BehaviorTree's static data and is serialized
/// separately.
/// </summary>
public class TreeRuntime {
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
                    NodeContexts[value.ID] = value.Context;
                }
            }
        }
    }

    public Dictionary<string, NodeContext> NodeContexts { get; } = new();

    public TreeRuntime(BehaviorTree tree) {
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
