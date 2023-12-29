using GBT.Sharp.Core.Nodes;
using MessagePack;
using MessagePack.Resolvers;
using NanoidDotNet;
using System.Buffers;

namespace GBT.Sharp.Core;

public class BehaviorTree {
    public static TreeLogger Logger { get; } = new TreeLogger();

    public string ID { get; private set; } = Nanoid.Generate();

    private TreeRuntime _context;
    private Node? _rootNode;

    public TreeRuntime Context {
        get => _context;
        set {
            if (_context != value) {
                _context = value;
                OnContextChanged();
            }
        }
    }

    public BehaviorTree(TreeRuntime? context = null) {
        _context = context ?? CreateContext();
    }

    public void SetRootNode(Node rootNode) {
        _rootNode = rootNode;
        _rootNode.Runtime = _context;
    }

    public void Tick() {
        if (_rootNode is null) {
            throw new InvalidOperationException("the tree has no root node");
        } else {
            if (Context.RunningNode is null) {
                Context.Trace.NewPass();
            }
            (Context.RunningNode ?? _rootNode).Tick();
        }
    }

    public void Interrupt() {
        if (Context.RunningNode is null) {
            return;
        }

        Context.Trace.Add(null, $"interrupt");
        Node? node = Context.RunningNode;
        while (node is not null) {
            node.Reset();
            node = node.Parent;
        }
        Context.RunningNode = null;
    }

    private TreeRuntime CreateContext() {
        return new TreeRuntime(this);
    }

    private void OnContextChanged() {
        Interrupt();
    }

    private TreeData WriteSavedData() {
        return new TreeData(
            ID: ID,
            Nodes: _rootNode?.Save(null).ToArray() ?? Array.Empty<NodeData>(),
            RootID: _rootNode?.ID ?? string.Empty);
    }
    private void ReadSavedData(TreeData data) {
        ID = data.ID;
        if (data.Nodes.Length > 0) {
            var nodeLoader = new NodeLoader();
            Dictionary<string, Node> nodes = nodeLoader.LoadAll(data.Nodes);
            if (string.IsNullOrEmpty(data.RootID)) {
                Logger.Warn("cannot set loaded root node because RootID is empty", null);
            } else if (nodes.TryGetValue(data.RootID, out Node? rootNode)) {
                SetRootNode(rootNode);
            } else {
                Logger.Warn($"cannot set loaded root node ({data.RootID}) because it was not loaded", null);
            }
        }
    }


    public void Save(IBufferWriter<byte> writer) {
        MessagePackSerializer.Serialize(writer, WriteSavedData());
    }
    public byte[] Save() {
        return MessagePackSerializer.Serialize(WriteSavedData());
    }
    public void Load(byte[] bin) {
        ReadSavedData(MessagePackSerializer.Deserialize<TreeData>(bin));
    }
    public void Load(ReadOnlyMemory<byte> buffer) {
        ReadSavedData(MessagePackSerializer.Deserialize<TreeData>(buffer));
    }
    public IEnumerable<Node> Flatten() {
        return _rootNode?.Flatten() ?? Enumerable.Empty<Node>();
    }
}