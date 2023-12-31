﻿using GBT.Sharp.Core.Nodes;
using GBT.Sharp.Core.Serialization;
using MessagePack;
using NanoidDotNet;
using System.Buffers;

namespace GBT.Sharp.Core;

public partial class BehaviorTree {
    public static TreeLogger Logger { get; } = new TreeLogger();

    public string ID { get; private set; } = Nanoid.Generate();

    private TreeRuntime _runtime;
    private Node? _rootNode;

    public TreeRuntime Runtime {
        get => _runtime;
        set {
            if (_runtime != value) {
                _runtime = value;
                OnRuntimeChanged();
            }
        }
    }

    public BehaviorTree(TreeRuntime? runtime = null) {
        _runtime = runtime ?? CreateRuntime();
    }

    public void SetRootNode(Node rootNode) {
        _rootNode = rootNode;
        _rootNode.Runtime = _runtime;
    }

    public void Tick() {
        if (_rootNode is null) {
            throw new InvalidOperationException("the tree has no root node");
        } else {
            if (Runtime.RunningNode is null) {
                Runtime.Trace.NewPass();
            }
            (Runtime.RunningNode ?? _rootNode).Tick();
        }
    }

    public void Interrupt() {
        if (Runtime.RunningNode is null) {
            return;
        }

        Runtime.Trace.Add(null, $"interrupt");
        Node? node = Runtime.RunningNode;
        while (node is not null) {
            node.Reset();
            node = node.Parent;
        }
        Runtime.RunningNode = null;
    }

    public IEnumerable<Node> Flatten() {
        return _rootNode?.Flatten() ?? Enumerable.Empty<Node>();
    }
    public Node? FindNode(string id) {
        return Flatten().FirstOrDefault(n => n.ID == id);
    }
    public Node? FindNodeByName(string name) {
        return Flatten().FirstOrDefault(n => n.Name == name);
    }

    private TreeRuntime CreateRuntime() {
        return new TreeRuntime(this);
    }

    private void OnRuntimeChanged() {
        Interrupt();
    }

    private Data WriteSavedData() {
        return new Data(
            ID: ID,
            Nodes: _rootNode?.Save(null).ToArray() ?? Array.Empty<Node.Data>(),
            RootID: _rootNode?.ID ?? string.Empty);
    }
    private void ReadSavedData(Data data) {
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
        ReadSavedData(MessagePackSerializer.Deserialize<Data>(bin));
    }
    public void Load(ReadOnlyMemory<byte> buffer) {
        ReadSavedData(MessagePackSerializer.Deserialize<Data>(buffer));
    }
}