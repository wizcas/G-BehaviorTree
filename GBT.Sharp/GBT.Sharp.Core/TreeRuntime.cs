using GBT.Sharp.Core.Diagnostics;
using GBT.Sharp.Core.Nodes;
using MessagePack;
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

    public Dictionary<string, NodeContext> NodeContexts { get; set; } = new();

    public TreeRuntime(BehaviorTree tree) {
        Tree = tree;
    }

    protected virtual Data WriteSavedData() {
        return Data.From(this);
    }
    protected virtual void ReadSavedData(Data data) {
        NodeContexts.Clear();
        IDictionary<string, Node> allNodes = Tree.Flatten().ToDictionary(n => n.ID);

        if (!string.IsNullOrEmpty(data.RunningNodeID)
            && allNodes.TryGetValue(data.RunningNodeID, out Node? runningNode)) {
            RunningNode = runningNode;
        } else {
            RunningNode = null;
        }
        foreach (NodeContext.Data contextData in data.NodeContexts) {
            NodeContexts[contextData.NodeID] = NodeContext.Data.Load(contextData, allNodes);
        }
    }

    public void Save(IBufferWriter<byte> writer) {
        MessagePackSerializer.Serialize(writer, WriteSavedData());
    }
    public byte[] Save() {
        return MessagePackSerializer.Serialize(WriteSavedData());
    }
    public void Load(byte[] bin) {
        ReadSavedData(MessagePackSerializer.Deserialize<Data>(bin));
    }
    public void Load(ReadOnlyMemory<byte> buffer) {
        ReadSavedData(MessagePackSerializer.Deserialize<Data>(buffer));
    }


    /// <summary>
    /// Persistable data for the TreeRuntime.
    /// </summary>
    [MessagePackObject(true)]
    public record struct Data(string TreeID, string? RunningNodeID) {
        public NodeContext.Data[] NodeContexts { get; set; } = Array.Empty<NodeContext.Data>();
        public static Data From(TreeRuntime runtime) {
            return new(runtime.Tree.ID, runtime.RunningNode?.ID) {
                NodeContexts = runtime.NodeContexts.Values.Select(NodeContext.Data.From).ToArray()
            };
        }
    }
}