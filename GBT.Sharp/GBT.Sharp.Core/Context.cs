using GBT.Sharp.Core.Diagnostics;
using GBT.Sharp.Core.Nodes;
using System.Buffers;

namespace GBT.Sharp.Core;

public interface ITreeContext {
    BehaviorTree Tree { get; init; }
    Trace Trace { get; }
    /// <summary>
    /// The node that is currently running if during a tick,
    /// or the node that will run next if the previous tick is finished.
    /// </summary>
    public Node? RunningNode { get; set; }
    /// <summary>
    /// Save the context to a persistent storage.
    /// </summary>
    Task Save(IBufferWriter<byte> writer);
    /// <summary>
    /// Called when a node begins to run for the first time in
    /// the current pass.
    /// </summary>
    void EnterNode(Node? node);
    /// <summary>
    /// Called when a node finishes running in the current pass by
    /// reaching a final state (success or failure).
    /// </summary>
    void ExitNode(Node node);
}

public class TreeContext : ITreeContext {
    public BehaviorTree Tree { get; init; }
    public Trace Trace { get; } = new();
    public Node? RunningNode { get; set; }

    public TreeContext(BehaviorTree tree) {
        Tree = tree;
    }
    public void EnterNode(Node? node) {
        Trace.Add(node, node is null ? "Running node cleared" : $"becomes running node");
        RunningNode = node;
    }
    public void ExitNode(Node node) {
        Trace.Add(node, $"exit");
        if (RunningNode != node) {
            BehaviorTree.Logger.Warn($"skip: try to exit running node {node} but the running node is {RunningNode}", node);
            return;
        }
        EnterNode(node.Parent);
        (node.Parent as IParentNode)?.OnChildExit(node);
    }

    public Task Save(IBufferWriter<byte> writer) {
        // TODO
        return Task.CompletedTask;
    }
}